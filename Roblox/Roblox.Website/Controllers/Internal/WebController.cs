using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roblox.Dto.Assets;
using Roblox.Exceptions;
using Roblox.Libraries.Assets;
using Roblox.Models.Assets;
using Roblox.Models.Groups;
using Roblox.Models.Staff;
using Roblox.Models.Thumbnails;
using Roblox.Models.Users;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using Roblox.Website.Filters;
using Roblox.Website.WebsiteModels.Catalog;
using Type = System.Type;

namespace Roblox.Website.Controllers;

[ApiController]
[Route("/")]
public class WebController : ControllerBase
{
    private static ControllerServices staticServices { get; } = new();
    static WebController()
    {
        // Init server close tasks
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    //await staticServices.gameServer.DeleteOldGameServers();
                }
                catch (Exception e)
                {
                    Console.WriteLine("[info] KillOldservers task failed: {0}\n{1}",e.Message,e.StackTrace);
                }
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        });
    }

    [HttpGet("userads/redirect")]
    public async Task<IActionResult> AdRedirect(string data)
    {
        // please ignore the "url" half of data string, it is legacy and should not be trusted
        var decoded = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(data));
        var arr = decoded.Split("|");
        var adId = long.Parse(arr[0]);
        var ad = await services.assets.GetAdvertisementById(adId);
        // if the ad isn't running, don't report it as a click.
        // maybe someone clicked after leaving their computer online overnight or something?
        if (ad.isRunning)
        {
            await services.assets.IncrementAdvertisementClick(ad.id);
        }
        switch (ad.targetType)
        {
            case UserAdvertisementTargetType.Asset:
                var itemData = await services.assets.GetAssetCatalogInfo(ad.targetId);
                var redirectUrl = "/catalog/" + itemData.id + "/" + UrlUtilities.ConvertToSeoName(itemData.name);
                return Redirect(redirectUrl);
            case UserAdvertisementTargetType.Group:
                return Redirect("/My/Groups.aspx?gid=" + ad.targetId);
            default:
                throw new NotImplementedException();
        }
    }

    [HttpGet("/users/favorites/list-json")]
    public async Task<dynamic> GetFavoritesLegacy(long userId, Models.Assets.Type assetTypeId, int pageNumber = 1,
        int itemsPerPage = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (itemsPerPage < 1 || itemsPerPage > 100) itemsPerPage = 10;
        
        // /users/favorites/list-json?assetTypeId=9&itemsPerPage=100&pageNumber=1&userId=3081467602
        var favs = await services.assets.GetFavoritesOfType(userId, assetTypeId, itemsPerPage,
            (itemsPerPage * pageNumber) - itemsPerPage);
        var details = (await services.assets.MultiGetInfoById(favs.Select(c => c.assetId))).ToList();
        var universeStuff =
            await services.games.MultiGetPlaceDetails(details.Where(c => c.assetType == Models.Assets.Type.Place)
                .Select(c => c.id));
        
        return new
        {
            IsValid = true,
            Data = new
            {
                Page = pageNumber,
                ItemsPerPage = itemsPerPage,
                PageType = "favorites",
                Items = details.Select(c =>
                {
                    var details = universeStuff.FirstOrDefault(x => x.placeId == c.id);
                    
                    return new
                    {
                        AssetRestrictionIcon = new
                        {
                            CssTag = c.itemRestrictions.Contains("LimitedUnique") ? "limited-unique" :
                                c.itemRestrictions.Contains("Limited") ? "limited" : "",
                        },
                        Item = new
                        {
                            AssetId = c.id,
                            UniverseId = details?.universeId,
                            Name = c.name,
                            AbsoluteUrl = "/catalog/" + c.id + "/--",
                            AssetType = (int) c.assetType,
                            AssetCategory = 0,
                            CurrentVersionId = 0,
                            LastUpdated = (string?) null,
                        },
                        Creator = new
                        {
                            Id = c.creatorTargetId,
                            Name = c.creatorName,
                            Type = (int) c.creatorType,
                            CreatorProfileLink = c.creatorType == CreatorType.Group
                                ? "/My/Groups.aspx?gid=" + c.creatorTargetId
                                : "/users/" + c.creatorTargetId + "/profile",
                        },
                        Product = new
                        {
                            PriceInRobux = c.price,
                            PriceInTickets = c.priceTickets,
                            IsForSale = c.isForSale,
                            Is18Plus = c.is18Plus,
                            IsLimited = c.itemRestrictions.Contains("Limited"),
                            IsLimitedUnique = c.itemRestrictions.Contains("LimitedUnique"),
                            IsFree = c.price == 0,
                        },
                    };
                }),
            },
        };
    }

    [HttpGet("users/inventory/list-json")]
    public async Task<dynamic> GetUserInventoryLegacy(long userId, Models.Assets.Type assetTypeId, string? cursor = "",
        int itemsPerPage = 10)
    {
        var count = await services.inventory.CountInventory(userId, assetTypeId);
        if (count == 0)
            return new
            {
                IsValid = true,
                Data = new
                {
                    TotalItems = 0,
                    nextPageCursor = (string?)null,
                    previousPageCursor = (string?)null,
                    PageType = "inventory",
                    Items = Array.Empty<int>(),
                }
            };
        int offset = !string.IsNullOrWhiteSpace(cursor) ? int.Parse(cursor) : 0;
        int limit = itemsPerPage;
        if (limit is > 100 or < 1) limit = 10;

        var canView = await services.inventory.CanViewInventory(userId, userSession?.userId ?? 0);
        if (!canView)
            return new
            {
                IsValid = false,
                Data = "User does not exist",
            };

        var result = (await services.inventory.GetInventory(userId, assetTypeId, "desc", limit, offset)).ToList();
        var moreAvailable = count > (offset + limit);

        return new
        {
            IsValid = true,
            Data = new
            {
                TotalItems = count,
                Start = 0,
                End = -1,
                Page = ((int) (offset / limit))+1,
                nextPageCursor = moreAvailable ? (offset + limit).ToString() : null,
                previousPageCursor = offset >= limit ? (offset - limit).ToString() : null,
                ItemsPerPage = limit,
                PageType = "inventory",
                Items = result.Select(c =>
                {
                    return new
                    {
                        AssetRestrictionIcon = new
                        {
                            CssTag = c.isLimitedUnique ? "limited-unique" : c.isLimited ? "limited" : "",
                        },
                        Item = new
                        {
                            AssetId = c.assetId,
                            UniverseId = (long?) null,
                            Name = c.name,
                            AbsoluteUrl = "/item-item?id=" + c.assetId,
                            AssetType = (int) c.assetTypeId,
                        },
                        Creator = new
                        {
                            Id = c.creatorId,
                            Name = c.creatorName,
                            Type = (int) c.creatorType,
                            CreatorProfileLink = c.creatorType == CreatorType.User
                                ? $"/users/{c.creatorId}/profile"
                                : $"/My/Groups.aspx?gid={c.creatorId}",
                        },
                        Product = new
                        {
                            PriceInRobux = c.originalPrice ?? 0,
                            SerialNumber = c.serialNumber,
                        },
                        PrivateSeller = (object?) null,
                        Thumbnail = new { },
                        UserItem = new { },
                    };
                }),
            },
        };
    }
    [HttpGet("users/{userId:long}")]
    public async Task<IActionResult> userInfo(long userId)
    {

        var userInfo = await services.users.GetUserById(userId);
        var data  =  new
        {
            Id = userId,
            Username = userInfo.username,
        };

        return Content(JsonConvert.SerializeObject(data), "application/json");
    }
    [HttpGet("users/{userId:long}/canmanage/{placeId:long}")]
    public async Task<IActionResult> CanManage(long userId, long placeId)
    {
        bool canManagePlace = await services.assets.CanUserModifyItem(placeId, userId);
        bool isOwner =  StaffFilter.IsOwner(userId);
        
        dynamic json = new { Success = canManagePlace || isOwner, CanManage = canManagePlace || isOwner };
        return Content(JsonConvert.SerializeObject(json), "application/json");
    }
    
    [HttpPost("users/set-builders-club")]
    public async Task SetBuildersClub(MembershipType membershipType)
    {
        if (userSession == null || !Enum.IsDefined(membershipType))
            return;
        
        await services.users.InsertOrUpdateMembership(userSession.userId, membershipType);
    }

    [HttpPost("asset/toggle-profile")]
    public async Task<dynamic> AddAssetToProfile([Required, FromBody] AddToProfileCollectionsRequest request)
    {
        var currentCollection = (await services.inventory.GetCollections(safeUserSession.userId)).ToList();
        if (request.addToProfile)
        {
            var ownsItem = await services.users.GetUserAssets(safeUserSession.userId, request.assetId);
            if (!ownsItem.Any())
                return new
                {
                    isValid = false,
                    data = new { },
                    error = "You do not own this item",
                };
            
            if (!currentCollection.Contains(request.assetId))
            {
                await services.inventory.SetCollections(safeUserSession.userId, currentCollection.Prepend(request.assetId).Distinct());
            }   
        }
        else
        {
            currentCollection.RemoveAll(c => c == request.assetId);
            await services.inventory.SetCollections(safeUserSession.userId, currentCollection);
        }

        return new
        {
            isValid = true,
            data = new { },
            error = "",
        };
    }

    [HttpGet("users/profile/robloxcollections-json")]
    public async Task<dynamic> GetUserCollections(long userId)
    {
        var result = (await services.inventory.GetCollections(userId)).ToList();
        if (result.Count < 1)
        {
            var inventory = await services.inventory.GetInventory(userId, Models.Assets.Type.Hat, "desc", 6, 0);
            result = inventory.Take(6).Select(c => c.assetId).ToList();
        }
        var items = (await services.assets.MultiGetInfoById(result)).ToArray();
        return new
        {
            CollectionsItems = result.Select(id =>
            {
                var c = items.First(i => i.id == id);
                return new
                {
                    Id = c.id,
                    AssetSeoUrl = $"/item-item?id=" + c.id,
                    Name = c.name,
                    FormatName = (string?) null,
                    Thumbnail = new
                    {
                        Final = true,
                        Url = $"/thumbs/asset.ashx?assetId={c.id}&width=420&height=420&format=png",
                        Id = c.id,
                    },
                    AssetRestrictionIcon = new
                    {
                        TooltipText = (string?) null,
                        CssTag = c.itemRestrictions.Contains("Limited") ? "limited" :
                            c.itemRestrictions.Contains("LimitedUnique") ? "limited-unique" : null,
                        LoadAssetRestrictionIconCss = false,
                        HasTooltip = false,
                    },
                };
            }),
        };
    }

    [HttpGet("comments/get-json")]
    public async Task<dynamic> GetAssetComments(long assetId, int startIndex)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.AssetCommentsEnabled);
        /*
        var details = (await services.assets.MultiGetAssetDeveloperDetails(new []{assetId})).First();
        if (!details.enableComments)
        {
            return new
            {
                IsUserModerator = false,
                Comments = new List<dynamic>(),
                MaxRows = 10,
                AreCommentsDisabled = true,
            };
        }
        */
        var com = await services.assets.GetComments(assetId, startIndex, 10);
        var isModerator = userSession != null && (await services.users.GetStaffPermissions(userSession.userId))
            .Any(a => a.permission == Access.DeleteComment);
        
        return new
        {
            IsUserModerator = isModerator,
            MaxRows = 10,
            AreCommentsDisabled = false,
            Comments = com.Select(c => new
            {
                Id = c.id,
                PostedDate = c.createdAt.ToString("MMM").Replace(".", "") + c.createdAt.ToString(" dd, yyyy | h:mm ") + c.createdAt.ToString("tt").ToUpper().Replace(".", ""),
                AuthorName = c.username,
                AuthorId = c.userId,
                Text = c.comment,
                ShowAuthorOwnsAsset = false,
                AuthorThumbnail = new
                {
                    AssetId = 0,
                    AssetHash = (string?) null,
                    AssetTypeId = 0,
                    Url = "/Thumbs/avatar.ashx?userId=" + c.userId,
                    IsFinal = true,
                },
            })
        };
    }

    [HttpPost("comments/post")]
    public async Task<dynamic> AddComment([Required, FromBody] AddCommentRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.AssetCommentsEnabled);
        try
        {
            await services.assets.AddComment(request.assetId, userSession.userId, request.text);
            return new
            {
                ErrorCode = (string?)null,
            };
        }
        catch (ArgumentException e)
        {
            return new
            {
                ErrorCode = e.Message,
            };
        }
    }

    [HttpGet("game/get-join-script")]
    public async Task<dynamic> GetJoinScript(long placeId)
    {
        // TODO: Rate limit, or caching, or something
        string clientVer;
        long year = await services.games.GetYear(placeId);

        switch(year)
        {
            case 2015:
                clientVer = "2015M";
                break;
            case 2016:
                clientVer = "2016E";
                break;
            case 2017:
                clientVer = "2017L";
                break;
            case 2018:
                clientVer = "2018L";
                break;
            case 2020:
                clientVer = "2020L";
                break;
            case 2021:
                clientVer = "2021M";
                break;
            default:
                clientVer = "2016E";
                break;
        }        
        var placeInfo = await services.assets.GetAssetCatalogInfo(placeId);
        if (placeInfo.assetType != Models.Assets.Type.Place) throw new BadRequestException();
        var modInfo = (await services.assets.MultiGetAssetDeveloperDetails(new[] {placeId})).First();
        if (modInfo.moderationStatus != ModerationStatus.ReviewApproved) throw new BadRequestException();
        var bootstrapperArgs = $":1+launchmode:play+clientversion:{clientVer}+gameinfo:{Request.Cookies[".ROBLOSECURITY"]}+placelauncherurl:{Configuration.BaseUrl}/Game/PlaceLauncher.ashx?request=RequestGame&placeId={placeId}&isPartyLeader=false&gender=&isTeleport=true+k:l+client";
        var args =
            $"--authenticationUrl {Roblox.Configuration.BaseUrl}/Login/Negotiate.ashx --authenticationTicket {Request.Cookies[".ROBLOSECURITY"]} --joinScriptUrl {Configuration.BaseUrl}/Game/PlaceLauncher.ashx?request=RequestGame&placeId={placeId}&isPartyLeader=false&gender=&isTeleport=true";
        return new
        {
            joinScriptUrl = bootstrapperArgs,
            prefix = "projex-player",
            retroArgs = args
        };
    }

    [HttpGet("game/get-join-script-fromjobid")]
    public async Task<dynamic> GetJoinScriptFromJobId(long placeId, string jobId)
    {
        string clientVer;
        long year = await services.games.GetYear(placeId);
        switch(year)
        {
            case 2015:
                clientVer = "2015M";
                break;
            case 2016:
                clientVer = "2016E";
                break;
            case 2017:
                clientVer = "2017L";
                break;
            case 2018:
                clientVer = "2018L";
                break;
            case 2020:
                clientVer = "2020L";
                break;
            case 2021:
                clientVer = "2021M";
                break;
            default:
                clientVer = "2016E";
                break;
        }          
        var placeInfo = await services.assets.GetAssetCatalogInfo(placeId);
        if (placeInfo.assetType != Models.Assets.Type.Place) throw new BadRequestException();
        var modInfo = (await services.assets.MultiGetAssetDeveloperDetails(new[] {placeId})).First();
        if (modInfo.moderationStatus != ModerationStatus.ReviewApproved) throw new BadRequestException();
        var bootstrapperArgs = $"://1+launchmode:play+clientversion:{clientVer}+gameinfo:{Request.Cookies[".ROBLOSECURITY"]}+placelauncherurl:{Configuration.BaseUrl}/Game/PlaceLauncher.ashx?request=RequestGameJob&placeId={placeId}&gameId={jobId}&isPartyLeader=false&gender=&isTeleport=true+k:l+client";
        var args =
            $"--authenticationUrl {Roblox.Configuration.BaseUrl}/Login/Negotiate.ashx --authenticationTicket {Request.Cookies[".ROBLOSECURITY"]} --joinScriptUrl {Configuration.BaseUrl}/Game/PlaceLauncher.ashx?request=RequestGameJob&placeId={placeId}&gameId={jobId}&isPartyLeader=false&gender=&isTeleport=true";
        return new
        {
            joinScriptUrl = bootstrapperArgs,
            prefix = "projex-player",
            retroArgs = args
        };
    }

    [HttpGet("usercheck/show-tos")]
    public dynamic GetIsTosCheckRequired()
    {
        return new
        {
            success = true,
        };
    }

    [HttpGet("games/getgameinstancesjson")]
    public async Task<dynamic> GetGameServers(long placeId, int startIndex)
    {
        var limit = 10;
        var offset = startIndex;
        var servers = (await services.gameServer.GetGameServers(placeId, offset, limit)).ToList();
        var details = (await services.games.MultiGetPlaceDetails(new[] { placeId })).First();
        List<dynamic> collection = new List<dynamic>();

        foreach (var server in servers)
        {
            var jobId = server.id; 
            var players = server.players.ToList();
            long Ping = await services.gameServer.GetServerStat(server.id); 
            collection.Add(new
            {
                placeId,
                Capacity = details.maxPlayerCount,
                Ping = Ping, 
                Fps = 60, 
                ShowSlowGameMessage = false, // todo
                UserCanJoin = true, // todo: false if vip server
                ShowShutdownButton = false, // todo: true if vip server player owns or user has perms
                jobId,
                FriendsMouseover = "",
                FriendsDescription = "",
                PlayersCapacity = $"{players.Count} of {details.maxPlayerCount}",
                RobloxAppJoinScript = "", // todo
                CurrentPlayers = players.Select(c => new
                {
                    Id = c.userId,
                    Username = c.username,
                    Thumbnail = new
                    {
                        IsFinal = true,
                        Url = "/Thumbs/Avatar-Headshot.ashx?userid=" + c.userId,
                    },
                }),
            });
        }

        return new
        {
            PlaceId = placeId,
            ShowShutdownAllButton = false, // todo: enable if user has perms
            Collection = collection,
            TotalCollectionSize = servers.Count,
        };
    }

    [HttpGet("search/users/results")]
    public async Task<dynamic> SearchUsersJson(string? keyword = null, int offset = 0, int limit = 10)
    {
        if (limit is > 100 or < 1)
            limit = 10;
        if ((offset / limit) > 1000)
            offset = 0;

        var result = (await services.users.SearchUsers(keyword, limit, offset)).ToArray();
        if (result.Length == 0)
            return new
            {
                Keyword = keyword,
                StartIndex = offset,
                MaxRows = limit,
                TotalResults = 0,
                UserSearchResults = Array.Empty<int>(),
            };
        // No DB pagination yet, it's just too expensive to be worth it right now
        var userInfo = await services.users.MultiGetUsersById(result.Skip(offset).Take(limit).Select(c => c.userId));
        return new
        {
            Keyword = keyword,
            StartIndex = offset,
            MaxRows = limit,
            TotalResults = result.Length,
            UserSearchResults = userInfo.Select(c => new
            {
                UserId = c.id,
                Name = c.name,
                DisplayName = c.displayName,
                Blurb = "",
                PreviousUserNamesCsv = "",
                IsOnline = false,
                LastLocation = (string?) null,
                UserProfilePageUrl = "/users/" + c.id + "/profile",
                LastSeenDate = (string?) null,
                PrimaryGroup = "",
                PrimaryGroupUrl = "",
            }),
        };
    }

    private static readonly List<Models.Assets.Type> AllowedAssetTypes = new()
    {
        Models.Assets.Type.Audio,
        Models.Assets.Type.TeeShirt,
        Models.Assets.Type.Shirt,
        Models.Assets.Type.Pants,
        Models.Assets.Type.Image,
        Models.Assets.Type.Video,
    };

    private static int pendingAssetUploads { get; set; } = 0;
    private static readonly Mutex pendingAssetUploadsMux = new();

    [HttpPost("develop/upload-version")]
    public async Task UploadVersion([Required, FromForm] UploadAssetVersionRequest request)
    {
        var info = await services.assets.GetAssetCatalogInfo(request.assetId);
        var canUpload = await services.assets.CanUserModifyItem(info.id, safeUserSession.userId);

        // You can only upload place files right now
        if (info.assetType != Models.Assets.Type.Place)
        {
            canUpload = false;
        }

        if (canUpload == false)
            throw new RobloxException(403, 0, "Unauthorized");

        lock (pendingAssetUploadsMux)
        {
            if (pendingAssetUploads >= 2)
                throw new RobloxException(429, 0, "TooManyRequests");
            pendingAssetUploads++;
        }

        try
        {
            var fs = request.file.OpenReadStream();
            if (!await AssetValidationV2(fs))
                throw new RobloxException(400, 0, "The asset file doesn't look correct. Please try again.");
            fs.Position = 0;

            await services.assets.CreateAssetVersion(request.assetId, safeUserSession.userId, fs);
            // Render in the background
            services.assets.RenderAsset(request.assetId, info.assetType);
        }
        finally
        {
            lock (pendingAssetUploadsMux)
            {
                pendingAssetUploads--;
            }
        }
    }
    private async Task<bool> AssetValidationV2(Stream stream)
    {
        byte[] buffer = new byte[7]; 
        await stream.ReadAsync(buffer, 0, buffer.Length);
        string startOfFile = Encoding.UTF8.GetString(buffer);
        return startOfFile == "<roblox";
    }
    [HttpPost("develop/upload")]
    public async Task<CreateResponse> UploadItem([Required, FromForm] UploadAssetRequest request)
    {
        FeatureFlags.FeatureCheck(FeatureFlag.UploadContentEnabled);
        if (!AllowedAssetTypes.Contains(request.assetType) || userSession == null) throw new BadRequestException();
        // flood check Start
        // 1 attempt every 5 seconds per user
        await services.cooldown.CooldownCheck("Develop:Upload:StartUserId:" + userSession.userId, TimeSpan.FromSeconds(5));
        // IP flood check too! same limit as userId for now
        await services.cooldown.CooldownCheck("Develop:Upload:StartIp:" + GetIP(), TimeSpan.FromSeconds(5));
        
        var isClothing = request.assetType is Models.Assets.Type.Shirt or Models.Assets.Type.Pants or Models.Assets.Type.TeeShirt;
        var isAudio = request.assetType is Models.Assets.Type.Audio;
        var isImage = request.assetType is Models.Assets.Type.Image;
        var isVideo = request.assetType is Models.Assets.Type.Video;

        if (!isClothing && !isAudio && !isImage && !isVideo)
            throw new RobloxException(400, 0, "Endpoint does not support this assetType: " + request.assetType);
        
        // Limit of 50 assets globally pending approval before failure
        var pendingAssets = await services.assets.CountAssetsPendingApproval();
        if (pendingAssets >= 50)
        {
            Metrics.UserMetrics.ReportGlobalPendingAssetsFloodCheckReached(userSession.userId);
            throw new RobloxException(400, 0, "There are too many pending items. Try again in a few minutes.");
        }
        
        var groupId = request.groupId == null ? 0 : request.groupId.Value;
        var creatorType = groupId == 0 ? CreatorType.User : CreatorType.Group;
        var creatorId = creatorType == CreatorType.User ? userSession.userId : groupId;
        // check perms
        if (creatorType == CreatorType.Group)
        {
            var hasPermission = await services.groups.DoesUserHavePermission(userSession.userId, groupId,
                GroupPermission.CreateItems);
            if (!hasPermission)
                throw new RobloxException(401, 0, "Unauthorized");
        }
        
        // Limit of 10 pending assets per user/group
        if (groupId == 0)
        {
            var myPendingItems =
                await services.assets.CountAssetsByCreatorPendingApproval(userSession.userId, CreatorType.User);
            if (myPendingItems >= 20)
            {
                Metrics.UserMetrics.ReportPendingAssetsFloodCheckReached(userSession.userId);
                throw new RobloxException(409, 0,
                    "You have uploaded too many items in a short period of time. Wait a few minutes and try again.");
            }
        }
        else
        {
            var myPendingItems =
                await services.assets.CountAssetsByCreatorPendingApproval(groupId, CreatorType.Group);
            if (myPendingItems >= 20)
            {
                Metrics.UserMetrics.ReportPendingAssetsFloodCheckReached(userSession.userId);
                throw new RobloxException(409, 0, "You have uploaded too many items in a short period of time. Wait a few minutes and try again.");
            }
        }
        // Global max of 5 pending asset uploads. To prevent people spamming stuff from a million IPs and accounts.
        // Note that this is not distributed right now, it's just local per server.
        lock (pendingAssetUploadsMux)
        {
            if (pendingAssetUploads >= 5)
            {
                Metrics.UserMetrics.ReportGlobalUploadsFloodcheckReached(userSession.userId);
                throw new RobloxException(409, 0, "There are too many pending assets at this time. Try again in a few minutes.");
            }
            pendingAssetUploads++;
        }

        try
        {
            if (isClothing)
            {
                var stream = request.file.OpenReadStream();
                var pictureData = await services.assets.ValidateClothing(stream, request.assetType);
                stream.Position = 0;
                if (pictureData == null)
                    throw new BadRequestException(0, "Invalid image file");
                stream.Position = 0;
                stream.Position = 0;
                using (var originalImage = new Bitmap(stream))
                {
                    var memoryStream = new MemoryStream();

                    using (var newBitmap = new Bitmap(originalImage.Width, originalImage.Height))
                    {
                        using (var graphics = Graphics.FromImage(newBitmap))
                        {
                            graphics.DrawImage(originalImage, 0, 0, originalImage.Width, originalImage.Height);
                        }
                        newBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    }

                    memoryStream.Position = 0;

                    var imageAsset = await services.assets.CreateAsset(request.file.FileName, request.assetType + " Image",
                        userSession.userId, creatorType, creatorId, memoryStream, Models.Assets.Type.Image,
                        Genre.All,
                        ModerationStatus.AwaitingApproval);

                    memoryStream.Position = 0;
                    await services.assets.InsertOrUpdateAssetVersionMetadataImage(imageAsset.assetVersionId, (int)memoryStream.Length,
                        pictureData.width, pictureData.height, pictureData.imageFormat,
                        await services.assets.GenerateImageHash(memoryStream));

                    var asset = await services.assets.CreateAsset(request.name, null, userSession.userId, creatorType, creatorId, null, request.assetType, Genre.All, imageAsset.moderationStatus, default,
                        default, default, default, imageAsset.assetId);

                    await services.users.CreateUserAsset(userSession.userId, asset.assetId);
                    
                    return asset;
                }
            }
            else if (isImage)
            {
                var stream = request.file.OpenReadStream();

                var pictureData = await services.assets.ValidateImage(stream);
                if (pictureData == null)
                    throw new BadRequestException(0, "Invalid image file");
                stream.Position = 0;
                using (var originalImage = new Bitmap(stream))
                {
                    var memoryStream = new MemoryStream();

                    using (var newBitmap = new Bitmap(originalImage.Width, originalImage.Height))
                    {
                        using (var graphics = Graphics.FromImage(newBitmap))
                        {
                            graphics.DrawImage(originalImage, 0, 0, originalImage.Width, originalImage.Height);
                        }

                        newBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    }

                   memoryStream.Position = 0;
                   var imageAsset = await services.assets.CreateAsset(request.name, "Image",
                        userSession.userId, creatorType, creatorId, memoryStream, Models.Assets.Type.Image,
                        Genre.All,
                        ModerationStatus.AwaitingApproval);

                    memoryStream.Position = 0;

                    await services.assets.InsertOrUpdateAssetVersionMetadataImage(imageAsset.assetVersionId, (int)memoryStream.Length,
                        pictureData.width, pictureData.height, pictureData.imageFormat,
                        await services.assets.GenerateImageHash(memoryStream));
                    
                    return imageAsset;
                }
            }
            else if (isAudio)
            {
                // check if has enough
                var balance = await services.economy.GetBalance(creatorType, creatorId);
                //choke is a audio uploader we dont need to check for the balance
                if (balance.robux < 100 && userSession.userId != 7)
                    throw new BadRequestException(0, "Not enough Robux for purchase");
                // validate auto

                var stream = request.file.OpenReadStream();
                stream.Position = 0;
                var ok = await services.assets.IsAudioValid(stream);
                stream.Position = 0;
                if (ok != AudioValidation.Ok)
                {
                    throw new BadRequestException(0, "Bad audio file. Error = " + ok.ToString());
                }
                // charge
                stream.Position = 0;
                await services.economy.ChargeForAudioUpload(creatorType, creatorId);
                stream.Position = 0;
                // create item
                var asset = await services.assets.CreateAsset(request.name, null, userSession.userId, CreatorType.User,
                    userSession.userId, stream, Models.Assets.Type.Audio, Genre.All, ModerationStatus.AwaitingApproval);
                return asset;
            }
            else if(isVideo)
            {
                // check if has enough
                var balance = await services.economy.GetBalance(creatorType, creatorId);
                if (balance.robux < 100 && userSession.userId != 7)
                    throw new BadRequestException(0, "Not enough Robux for purchase");
                // validate auto

                var stream = request.file.OpenReadStream();
                stream.Position = 0;
                var ok = await services.assets.IsVideoValid(stream);
                stream.Position = 0;
                if (ok != VideoValidation.Ok)
                {
                    throw new BadRequestException(0, "Bad video file. Error = " + ok.ToString());
                }
                // charge
                stream.Position = 0;
                await services.economy.ChargeForVideoUpload(creatorType, creatorId);
                stream.Position = 0;
                // create item
                var asset = await services.assets.CreateAsset(request.name, null, userSession.userId, CreatorType.User,
                    userSession.userId, stream, Models.Assets.Type.Video, Genre.All, ModerationStatus.AwaitingApproval);
                return asset;
            }

            throw new BadRequestException(0, "Invalid assetType");
        }
        finally
        {
            lock (pendingAssetUploadsMux)
            {
                pendingAssetUploads--;
            }
        }
    }

}