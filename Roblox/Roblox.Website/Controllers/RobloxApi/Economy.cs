using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roblox.Dto.Assets;
using Roblox.Dto.Users;
using Roblox.Exceptions;
using Roblox.Models.Assets;
using Roblox.Models.Economy;
using Roblox.Models.Groups;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using BadRequestException = Roblox.Exceptions.BadRequestException;
using ServiceProvider = Roblox.Services.ServiceProvider;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/")]
public class Economy : ControllerBase
{
    private void FeatureCheck()
    {
        FeatureFlags.FeatureCheck(FeatureFlag.EconomyEnabled);
    }
    
    [HttpGetBypass("v1/users/{userId:long}/currency")]
    public async Task<dynamic> GetUserCurrency(long userId)
    {
        FeatureCheck();
        if (userSession == null || userId != userSession.userId)
            throw new ForbiddenException();
        return await services.economy.GetUserBalance(userId);
    }

    [HttpGetBypass("v1/assets/{assetId}/users/{userId}/resellable-copies")]
    public async Task<dynamic> GetUserResellableCopiesOfAsset(long userId, long assetId)
    {
        FeatureCheck();
        if (userId != safeUserSession.userId)
            throw new ForbiddenException();

        var entries = await services.users.GetUserAssets(userId, assetId);
        return new
        {
            data = entries.Select(c =>
            {
                return new
                {
                    userAssetId = c.userAssetId,
                    seller = new
                    {
                        id = safeUserSession.userId,
                        name = safeUserSession.username,
                        type = CreatorType.User,
                    },
                    price = c.price,
                    serialNumber = c.serial,
                };
            }),
        };
    }

    [HttpPatch("assets/{assetId}/resellable-copies/{userAssetId}")]
    public async Task SetPriceOfUserAsset(long assetId, long userAssetId, [Required, FromBody] SetPriceRequest request)
    {
        FeatureCheck();
        // Confirm the user actually owns the item
        var ownedItems = await services.users.GetUserAssets(safeUserSession.userId, assetId);
        var isOwned = ownedItems.ToList().Find(c => c.userAssetId == userAssetId);
        if (isOwned == null)
            throw new ForbiddenException();
        // Confirm item can be sold

        var details = await services.assets.GetAssetCatalogInfo(assetId);
        var rsData = await services.assets.GetResaleData(assetId);
        
        if (details.itemRestrictions == null || details.isForSale || !details.itemRestrictions.Contains("Limited") && !details.itemRestrictions.Contains("LimitedUnique"))
        {
            // Item cannot be sold at this time
            throw new BadRequestException();
        }

        if(request.price != 0)
        {
            // Anti lpp
            if (rsData.recentAveragePrice != 0 && request.price < (rsData.recentAveragePrice * 0.60)) {
                throw new BadRequestException();
            }

            // Check if the request price is less than OG price
            if (request.price < details.price) {
                throw new BadRequestException();
            }
        }

        // Update price
        await services.users.SetPriceOfUserAsset(userAssetId, safeUserSession.userId, request.price);
    }

    [HttpGetBypass("v1/users/{userId}/revenue/summary/{timePeriod}")]
    public async Task<EconomySummary> GetMyRevenueSummary(string timePeriod)
    {
        FeatureCheck();
        // Ugliest one-liner in history award goes to...
        var startDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(timePeriod == "day" ? 1 : timePeriod == "week" ? 7 : timePeriod == "month" ? 30 : timePeriod == "year" ? 365 : 0));
        return await services.users.GetTransactionSummary(safeUserSession.userId, startDate);
    }

    private async Task PurchaseResellableItem(long assetId, PurchaseRequest request)
    {
        if (request.userAssetId == null)
            throw new BadRequestException(0, "BadRequest"); // how are we here?
        var userAsset = await services.users.GetUserAssetById(request.userAssetId.Value);
        // Confirm that the assetId provided in the request matches the asset id stored in the db
        if (userAsset.assetId != assetId)
            throw new BadRequestException(0, "assetId does not match provided assetId");
        // Confirm the user is not trying to buy their own item
        if (userAsset.userId == safeUserSession.userId)
            throw new BadRequestException(0, "BadRequest");
        // Check item data
        var itemData = await services.assets.GetAssetCatalogInfo(assetId);
        // Confirm item is limited
        if (!itemData.itemRestrictions.Contains("Limited") && !itemData.itemRestrictions.Contains("LimitedUnique"))
            throw new Exception("Cannot purchase non-limited and non limited-unique item");
        // If item is still for sale, it cannot be re-sold/purchased yet
        if (itemData.isForSale)
            throw new Exception("Item is still for sale");
        // If the price of the item does not match the price the requester expected, inform them the price changed
        if (userAsset.price == 0 || userAsset.price != request.expectedPrice)
            throw new Exception("Price has changed");
        // If the seller has changed, inform them the seller changed.
        // I'm not really sure how useful this is, but it's what Roblox does, so I'll leave it
        if (userAsset.userId != request.expectedSellerId)
            throw new Exception("Seller has changed");
        // Check if buyer has enough currency
        // Accuracy is not too important since this is re-checked anyway
        var buyerCurrency = await services.economy.GetUserRobux(safeUserSession.userId);
        // If user does not have enough money, they can't buy the item!
        if (buyerCurrency < userAsset.price)
            throw new BadRequestException(0, "BadRequest");
        // All validation logic seems to be complete. Let's do the actual transaction.
        await services.users.PurchaseResellableItem(safeUserSession.userId, userAsset.userAssetId);
    }

    private async Task PurchaseNormalItem(long assetId, PurchaseRequest request)
    {
        // Note: A lot of validation is duplicated in both this function and the transaction function. This is done because transactions and extra locks are slow - if someone starts spamming purchases where they don't have enough Robux, they could DOS transaction handling. If we do a ton of IF checks before starting the PG transaction, it's far less likely they'd take down our database (or cause unnecessary locks, breaking purchases for real customers, etc...)
        
        // First, confirm user does not own item already (When purchase normal is used, user can only own one copy of the assetId at any given time)
        var ownedCopies = (await services.users.GetUserAssets(safeUserSession.userId, assetId)).ToList();
        if (ownedCopies.Count != 0)
            throw new BadRequestException(0, "Asset is already owned");
        if (await services.users.HasUserPurchasedAssetBefore(safeUserSession.userId, assetId))
            throw new BadRequestException(0, "Asset has already been purchased");
        // Get the item data
        var details = await services.assets.GetAssetCatalogInfo(assetId);
        // Confirm it is for sale
        if (!details.isForSale)
            throw new BadRequestException(0, "Item is no longer for sale");
        // Confirm item is still for sale, if it has an offsale deadline
        var deadline = details.offsaleDeadline;
        if (deadline != null)
        {
            var isAfterDeadline = DateTime.UtcNow >= deadline;
            if (isAfterDeadline)
            {
                // Mark item as no longer for sale, then error
                await services.assets.UpdateItemIsForSale(details.id, false);
                throw new BadRequestException(0, "Item is no longer for sale");
            }
        }
        // Confirm price matches
        var userBalance = await services.economy.GetUserBalance(safeUserSession.userId);
        if (request.expectedCurrency == CurrencyType.Tickets)
        {
            // If an item has null ticket price, that means you cannot buy it for tickets
            if (details.priceTickets == null)
                throw new BadRequestException(0, "Item is no longer for sale");
            if (details.priceTickets != request.expectedPrice)
                throw new BadRequestException(0, "Price has changed");
            // Confirm user has enough tickets
            if (request.expectedPrice > userBalance.tickets)
                throw new BadRequestException(0, "Not enough tickets");
        }
        else if (request.expectedCurrency == CurrencyType.Robux)
        {
            // Note: if price is zero, currency is Robux, and item is for sale, that means the item is free.
            if (details.price != request.expectedPrice)
                throw new BadRequestException(0, "Price has changed");
            // Confirm user has enough robux
            if (details.price > userBalance.robux)
                throw new BadRequestException(0, "Not enough robux");
        }
        else
        {
            throw new NotImplementedException(); // Unexpected branch
        }
        // Confirm seller matches
        if (details.creatorTargetId != request.expectedSellerId)
            throw new BadRequestException(0, "Seller has changed");

        // If has a specific sale count...
        if (details.serialCount != null && details.serialCount != 0)
        {
            var soldCopies = await services.users.CountSoldCopiesForAsset(details.id);
            var maxCopiesToSell = (int) details.serialCount;
            // If we have sold more than available, mark as not for sale then error
            if (soldCopies >= maxCopiesToSell)
            {
                await services.assets.UpdateItemIsForSale(details.id, false);
                throw new BadRequestException(0, "Item is no longer for sale");
            }
        }
        
        // Everything seems ok now, so purchase the item
        await services.users.PurchaseNormalItem(safeUserSession.userId, assetId, request.expectedCurrency);
    }
    [HttpGetBypass("v1/products/{assetId:long}")]
    public async Task<dynamic> GetProductInfo(long assetId)
    {
        var details = await services.assets.GetAssetCatalogInfo(assetId);
        return new
        {
            purchasable = details.isForSale,
            reason = "Success",
            productId = details.id,
            price = details.price,
            currency = 1,
            assetId = details.id,
            assetName = details.name,
            assetType = details.assetType,
            assetTypeDisplayName = details.assetType,
            assetIsWearable = true,
            sellerName = details.creatorName,
            transactionVerb = "bought",
            isMultiPrivateSale = false
        };
    }
    /// <summary>
    /// Purchase an asset.
    /// </summary>
    /// <remarks>
    /// Note that we use assetId instead of productId in url, however, all our endpoints return an assetId instead of a productId for the productId param, so you are unlikely to need to code workarounds unless you hard-coded any productIds from Roblox.
    /// </remarks>
    [HttpPostBypass("v1/purchases/products/{assetId:long}")]
    public async Task<dynamic> PurchaseAsset(long assetId)
    {
        dynamic request;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            var json = await reader.ReadToEndAsync();
            Console.WriteLine(json);
            string fixedJson = json.Trim();
            if (fixedJson.StartsWith("{") && !fixedJson.EndsWith("}"))
            {
                fixedJson += "}";
                Console.WriteLine(fixedJson);
            }
            request = JsonConvert.DeserializeObject<PurchaseRequest>(fixedJson);
        }
        var details = await services.assets.GetAssetCatalogInfo(assetId);
        FeatureCheck();
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        // some sanity checks
        Console.WriteLine(request.expectedPrice + request.expectedSellerId);
        if (request.expectedSellerId == safeUserSession.userId)
            throw new RobloxException(400, 0, "Bad userId");
        if (request.userAssetId is 0 or < 0)
            request.userAssetId = null;
        // Confirm asset is buyable
        var user18Plus = await services.users.Is18Plus(safeUserSession.userId);
        if (!user18Plus)
        {
            if (await services.assets.Is18Plus(assetId))
                throw new RobloxException(400, 0,
                    "You cannot purchase 18+ items until you confirm you are 18 or over.");
        }
        /*   
        if (request.userAssetId != null)
        {
            // User is making UAID purchase
            await PurchaseResellableItem(assetId, request);
            // Update sellers avatar in background (in case they were wearing the item they sold)
            await Task.Run(async () =>
            {
                using var avatarService = ServiceProvider.GetOrCreate<AvatarService>();
                await avatarService.RedrawAvatar(request.expectedSellerId);
            });
        }
        */
        else
        {
            // User is making normal purchase
            request.expectedSellerId = details.creatorTargetId;
            await PurchaseNormalItem(assetId, request);
        }

        stopwatch.Stop();
        // Report time
        Metrics.EconomyMetrics.ReportItemPurchaseTime(stopwatch.ElapsedMilliseconds,
            request.userAssetId != null);
        return new
        {
            purchased = true,
            reason = "Success",
            productId = assetId,
            currency = 1,
            price = request.expectedPrice,
            assetId = assetId,
            assetName = details.name,
            assetIsWearable = true,
            sellerName = request.expectedSellerId.ToString(),
            transactionVerb = "bought",
            isMultiPrivateSale = false,
        };
    }

    [HttpGetBypass("v1/assets/{assetId:long}/resellers")]
    public async Task<dynamic> GetResellers(long assetId)
    {
        FeatureCheck();
        var result = await services.users.GetResellers(assetId);
        return new
        {
            nextPageCursor = (string?) null,
            previousPageCursor = (string?) null,
            data = result.Select(c => new
            {
                userAssetId = c.userAssetId,
                seller = new
                {
                    id = c.userId,
                    type = CreatorType.User,
                    name = c.username,
                },
                price = c.price,
                serialNumber = c.serialNumber,
            }),
        };
    }

    [HttpGetBypass("v1/assets/{assetId:long}/resale-data")]
    public async Task<AssetResaleData> GetResaleData(long assetId)
    {
        FeatureCheck();
        return await services.assets.GetResaleData(assetId);
    }

    [HttpGetBypass("v1/groups/{groupId:long}/addfunds/allowed")]
    public dynamic CanAddFundsToGroup()
    {
        FeatureCheck();
        return false;
    }

    [HttpGetBypass("v1/groups/{groupId:long}/currency")]
    public async Task<UserEconomy> GetGroupCurrency(long groupId)
    {
        FeatureCheck();
        var canViewFunds =
            await services.groups.DoesUserHavePermission(safeUserSession.userId, groupId,
                GroupPermission.SpendGroupFunds);
        if (!canViewFunds)
            throw new RobloxException(401, 0, "Unauthorized");
        
        return await services.economy.GetBalance(CreatorType.Group, groupId);
    }
    
    [HttpGetBypass("v1/groups/{groupId}/users-payout-eligibility")]
    public async Task<dynamic> GetUserPayoutEligibility(long groupId, string userIds)
    {
        var ids = userIds.Split(",").Select(long.Parse).Distinct();
        var result = new Dictionary<string, string>();
        foreach (var id in ids)
        {
            result[id.ToString()] = "Eligible";
        }
        return new
        {
            usersGroupPayoutEligibility = result,
        };
    }

    [HttpGetBypass("v1/groups/{groupId:long}/revenue/summary/{timePeriod}")]
    public dynamic GetGroupRevenueSummary(long groupId, string timePeriod)
    {
        FeatureCheck();
        return new
        {
            premiumPayouts = 0,
            groupPremiumPayouts = 0,
            recurringRobuxStipend = 0,
            itemSaleRobux = 0,
            purchasedRobux = 0,
            tradeSystemRobux = 0,
            pendingRobux = 0,
            groupPayoutRobux = 0,
            individiualToGroupRobux = 0,
        };
    }
}