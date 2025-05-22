using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roblox.Dto.Assets;
using Roblox.Dto.Thumbnails;
using Roblox.Exceptions;
using Roblox.Libraries.Assets;
using Roblox.Models;
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
using Roblox.Website.WebsiteModels.Thumbnails;
using Type = System.Type;

namespace Roblox.Website.Controllers;
[ApiController]
[Route("/")]
public class RbxThumbnails : ControllerBase
{
    public enum ThumbnailType
    {
        UserHeadshot = 1,
        UserAvatar,
        Asset,
        PlaceIcon
    }
    private async Task<RedirectResult> GetThumbnailUrl(long id, ThumbnailType type)
    {
        var authUser18Plus = userSession != null && await services.users.Is18Plus(userSession.userId);
        if (!authUser18Plus)
        {
            var avatar18Plus = await services.avatar.IsUserAvatar18Plus(id);
            if (avatar18Plus)
                return new RedirectResult("/img/blocked.png", false);
        }

        List<ThumbnailEntry> result = null;

        switch (type)
        {
            case ThumbnailType.UserHeadshot:
                result = (await services.thumbnails.GetUserHeadshots(new[] { id })).ToList();
                break;
            case ThumbnailType.UserAvatar:
                result = (await services.thumbnails.GetUserThumbnails(new[] { id })).ToList();
                break;
            case ThumbnailType.Asset:
                result = (await services.thumbnails.GetAssetThumbnails(new[] { id })).ToList();
                break;
            case ThumbnailType.PlaceIcon:
                long universeId = await services.games.GetUniverseId(id);
                result = (await services.thumbnails.GetGameIcons(new[] { universeId })).ToList();
                break;
        }

        var imageUrl = result?.FirstOrDefault()?.imageUrl ?? "/img/placeholder.png";
        return new RedirectResult(imageUrl, false);
    }
    //avatar stuff
    [HttpGetBypass("avatar-thumbnail/image")]
    [HttpGetBypass("thumbs/avatar.ashx")]
    public async Task<RedirectResult> GetAvatarThumbnail(long userId, string? username)
    {
        if (username != null)
        {
            try
            {
                userId = await services.users.GetUserIdFromUsername(username);
            }
            catch (Exception)
            {
                return new RedirectResult("/img/blocked.png", false);
            }
        }
        return await GetThumbnailUrl(userId, ThumbnailType.UserAvatar);
    }
    //headshot stuff
    [HttpGetBypass("Thumbs/Head.ashx")]
    [HttpGetBypass("headshot-thumbnail/image")]
    [HttpGetBypass("thumbs/avatar-headshot.ashx")]
    public async Task<RedirectResult> GetAvatarHeadShot(long userId)
    {
        return await GetThumbnailUrl(userId, ThumbnailType.UserHeadshot);
    }
    //place icon
    [HttpGetBypass("Thumbs/PlaceIcon.ashx")]
    [HttpGetBypass("Thumbs/GameIcon.ashx")]
    public async Task<RedirectResult> GetGameIcon(long assetId)
    {
        return await GetThumbnailUrl(assetId, ThumbnailType.PlaceIcon);
    }
    //asset icon stuff
    [HttpGet("asset-thumbnail/image")]
    [HttpGetBypass("icons/asset.ashx")]
    [HttpGetBypass("Game/Tools/ThumbnailAsset.ashx")]
    [HttpGetBypass("thumbs/asset.ashx")]
    public async Task<RedirectResult> GetAssetThumbnail(long assetId, long? aid)
    {        
        if(aid != null)
            assetId = (long)aid;
        return await GetThumbnailUrl(assetId, ThumbnailType.Asset);
    }
    //all json thumbnail apis
    [HttpGetBypass("avatar-thumbnail/json")]
    public async Task<dynamic> GetAvatarThumbnailJson([Required] long userId)
    {
        var result = (await services.thumbnails.GetUserThumbnails(new[] {userId})).ToList();
        return new
        {
            Url = $"{Configuration.BaseUrl}{result[0].imageUrl}",
            Final = true,
            SubstitutionType = 0
        };
    }
    [HttpGetBypass("asset-thumbnail/json")]
    public async Task<dynamic> GetAssetThumbnailJson([Required] long assetId)
    {
        var result = (await services.thumbnails.GetAssetThumbnails(new[] {assetId})).ToList();
        return new
        {
            Url = $"{Configuration.BaseUrl}{result[0].imageUrl}",
            Final = true,
            SubstitutionType = 0
        };
    }     

    [HttpGetBypass("asset-gameicon/multiget")]
    public async Task<dynamic> GetGameIconMultiGet([FromQuery] List<long> universeId)
    {
        return await services.thumbnails.GetGameIconsRBX(universeId);
    }
    [HttpGetBypass("v1/games/icons")]
    public async Task<RobloxCollection<ThumbnailEntry>> GetGameIcons(string universeIds)
    {
        var parsed = universeIds.Split(",").Select(long.Parse).Distinct().ToList();
        if (parsed.Count is > 200 or < 0) throw new BadRequestException();
        var result = await services.thumbnails.GetGameIcons(parsed);
        var result2 = result.Select(thumbnail => 
            new ThumbnailEntry
            {
                targetId = thumbnail.targetId,
                imageUrl = Configuration.BaseUrl + thumbnail.imageUrl,
                state = ThumbnailState.Completed,
            }).ToList();
        return new()
        {
            data = result2,
        };
    }
    [HttpGet("v1/users/avatar-headshot")]
    public async Task<RobloxCollection<ThumbnailEntry>> GetMultiHeadshot(string userIds)
    {
        var parsed = userIds.Split(",").Select(long.Parse).Distinct().ToList();
        if (parsed.Count is > 200 or < 0) throw new BadRequestException();
        var result = (await services.thumbnails.GetUserHeadshots(parsed)).ToList();
        var result2 = result.ToList();
        var authUser18Plus = userSession != null && await services.users.Is18Plus(userSession.userId);
        if (!authUser18Plus)
        {
            foreach (var item in result)
            {
                if (item.imageUrl is null) continue;

                var avatar18Plus = await services.avatar.IsUserAvatar18Plus(item.targetId);
                if (avatar18Plus)
                {
                    item.state = ThumbnailState.Blocked;
                    item.imageUrl = "/img/blocked.png";
                }
                else
                {
                    item.imageUrl = Configuration.BaseUrl + item.imageUrl;
                }
            }
        }
        else
        {
            foreach (var item in result)
            {
                if (item.imageUrl is null) continue;

                item.imageUrl = Configuration.BaseUrl + item.imageUrl;
            }
        }
        return new()
        {
            data = result2,
        };
    }
    [HttpPostBypass("v1/batch")]
    public async Task<dynamic> BatchThumbnailsRequest()
    {
        bool isGzip = Request.Headers["Content-Encoding"].ToString() == "gzip";
        IEnumerable<BatchRequestEntry> requestEntries;
        var tasks = new List<Task<IEnumerable<dynamic>>>();
        Console.WriteLine(isGzip);
        if (isGzip)
        {
            using (var decompressedStream = new MemoryStream())
            {
                using (var requestStream = Request.Body)
                {
                    using (var gzipStream = new GZipStream(requestStream, CompressionMode.Decompress))
                    {
                        await gzipStream.CopyToAsync(decompressedStream);
                    }
                }
                decompressedStream.Seek(0, SeekOrigin.Begin);

                using (var reader = new StreamReader(decompressedStream, Encoding.UTF8))
                {
                    var json = await reader.ReadToEndAsync();
                    Console.WriteLine(json);
                    requestEntries = JsonConvert.DeserializeObject<IEnumerable<BatchRequestEntry>>(json);
                }
            }
        }
        else
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var json = await reader.ReadToEndAsync();
                Console.WriteLine(json);
                requestEntries = JsonConvert.DeserializeObject<IEnumerable<BatchRequestEntry>>(json);
            }
        }

        var thumbs = requestEntries.ToList();
        var allResults = await Task.WhenAll(new List<Task<IEnumerable<dynamic>>>()
        {
            ThumbnailsControllerV1.MultiGetThumbnailsGeneric(thumbs, "Avatar", services.thumbnails.GetUserThumbnails),
            ThumbnailsControllerV1.MultiGetThumbnailsGeneric(thumbs, "AvatarThumbnail", services.thumbnails.GetUserThumbnails),
            ThumbnailsControllerV1.MultiGetThumbnailsGeneric(thumbs, "AvatarHeadShot", services.thumbnails.GetUserHeadshots),
            ThumbnailsControllerV1.MultiGetThumbnailsGeneric(thumbs, "GameIcon", services.thumbnails.GetGameIcons),
            ThumbnailsControllerV1.MultiGetThumbnailsGeneric(thumbs, "GameThumbnail", services.thumbnails.GetAssetThumbnails),
            ThumbnailsControllerV1.MultiGetThumbnailsGeneric(thumbs, "Asset", services.thumbnails.GetAssetThumbnails),
            ThumbnailsControllerV1.MultiGetThumbnailsGeneric(thumbs, "AssetThumbnail", services.thumbnails.GetAssetThumbnails),
            ThumbnailsControllerV1.MultiGetThumbnailsGeneric(thumbs, "GroupIcon", services.thumbnails.GetGroupIcons),
        });
        return new RobloxCollection<dynamic>()
        {
            data = allResults.SelectMany(x => x),
        };

        /*
        foreach (var entry in requestEntries)
        {
            switch (entry.type)
            {
                case "Avatar":
                    tasks.Add(ThumbnailsControllerV1.MultiGetThumbnailsGeneric(
                        thumbs, 
                        "Avatar", 
                        ids => services.thumbnails.GetUserThumbnails(new[] { entry.targetId })
                    ));
                    break;
                case "AvatarThumbnail":
                    tasks.Add(ThumbnailsControllerV1.MultiGetThumbnailsGeneric(
                        thumbs, 
                        "AvatarThumbnail", 
                        ids => services.thumbnails.GetUserThumbnails(new[] { entry.targetId })
                    ));
                    break;
                case "AvatarHeadShot":
                    await ThumbnailsControllerV1.MultiGetThumbnailsGeneric(thumbs, "AvatarHeadShot", services.thumbnails.GetUserHeadshots);
                    break;
                case "GameIcon":
                    tasks.Add(ThumbnailsControllerV1.MultiGetThumbnailsGeneric(
                        thumbs, 
                        "GameIcon", 
                        ids => services.thumbnails.GetGameIcons(new[] { entry.targetId })
                    ));
                    break;
                case "AssetThumbnail":
                    tasks.Add(ThumbnailsControllerV1.MultiGetThumbnailsGeneric(
                        thumbs, 
                        "AssetThumbnail", 
                        ids => services.thumbnails.GetUserHeadshots(new[] { entry.targetId })
                    ));
                    break;
                default:
                    tasks.Add(ThumbnailsControllerV1.MultiGetThumbnailsGeneric(thumbs, "AvatarHeadShot", services.thumbnails.GetUserHeadshots));
                    break;
            }
            
        }
        

        var allResults = await Task.WhenAll(tasks);

        
        var resultObject = new
        {
            data = allResults
        };
        var resultJson = JsonConvert.SerializeObject(resultObject, Formatting.Indented);
        Console.WriteLine($"Full Response: {resultJson}");
        return new
        {
            data = allResults.SelectMany(x => x),
        };
        */
    }
}

