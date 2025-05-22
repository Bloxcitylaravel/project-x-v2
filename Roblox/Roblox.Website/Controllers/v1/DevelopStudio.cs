using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Roblox.Dto.Games;
using Roblox.Models;
using Roblox.Models.Assets;
using Roblox.Models.Studio;
namespace Roblox.Website.Controllers;
[ApiController]
[Route("/v1")]
public class DevelopStudio : ControllerBase
{
    [HttpGet("gametemplates")]
    public dynamic StudioTemplates()
    {
        var Templates = new
        {
            gameTemplateType = "Generic",
            hasTutorials = false,
            universe = new Universe
            {
                id = 1,
                name = "Starter place",
                description = null,
                isArchived = false,
                rootPlaceId = 4430,
                isActive = true,
                privacyType = "Public",
                creatorType = "User",
                creatorTargetId = 3,
                creatorName = "shikataganai",
                created = DateTime.Parse("2013-11-01T08:47:14.07Z"),
                updated = DateTime.Parse("2023-05-02T22:03:01.107Z")
            }
        };
        var data = new { data = new[] { Templates } };
        string json = JsonConvert.SerializeObject(data);
        return Content(json, "application/json");
    }
    [HttpGet("search/universes")]
    public async Task<RobloxCollectionPaginated<GamesForCreatorDevelop>> GetUserCreatedGames()
    {
        int offset = int.Parse("0");
        var result =
            (await services.games.GetGamesForTypeDevelop(CreatorType.User, userSession.userId, userSession.username, 50, offset, null, null)).ToList();
        return new RobloxCollectionPaginated<GamesForCreatorDevelop>()
        {
            data = result
        };
    }
    [HttpGet("user/universes")]
    public async Task<RobloxCollectionPaginated<GamesForCreatorDevelop>> GetUserCreatedGames(string? sortOrder, string? accessFilter, int limit, string? cursor = null)
    {
        if (limit is > 100 or < 1) limit = 10;
        int offset = int.Parse(cursor ?? "0");
        var result =
            (await services.games.GetGamesForTypeDevelop(CreatorType.User, userSession.userId, userSession.username, limit, offset, sortOrder ?? "asc", accessFilter ?? "All")).ToList();
        return new RobloxCollectionPaginated<GamesForCreatorDevelop>()
        {
            nextPageCursor = result.Count >= limit ? (offset+limit).ToString(): null,
            previousPageCursor = offset >= limit ? (offset-limit).ToString() : null,
            data = result
        };
    }
    [HttpGet("universes/{universeId}/places")]
    public async Task<dynamic> GetUniverseAttachedPlaces(long universeId)
    {
        var uni = (await services.games.MultiGetUniverseInfo(new[] {universeId})).FirstOrDefault();
        return new 
        {
            previousPageCursor = (string?)null,
            nextPageCursor = (string?)null,
            data = new List<object>
            {
                new
                {
                    id = uni.rootPlaceId,
                    universeId = universeId,
                    name = uni.name,
                    description = uni.description
                }
            }
        };
    }
    [HttpGet("universes/{universeId}/permissions")]
    public async Task<dynamic> CanManage(long universeId)
    {
        var place = await services.games.GetRootPlaceId(universeId);
        bool canManage = await services.assets.CanUserModifyItem(place, safeUserSession.userId);
        return new
        {
            canManage,
            canCloudEdit = canManage
        };
    }
    [HttpGet("universes/{universeId}")]    
    public async Task<dynamic> UniverseInfo(long universeId)
    {
        var uni = (await services.games.MultiGetUniverseInfo(new[] {universeId})).FirstOrDefault();
        var assetInfo = (await services.assets.MultiGetAssetDeveloperDetails(new[] {uni.rootPlaceId})).First();
        return new
        {
            id = universeId,
            name = uni.name,
            description = uni.description,
            isArchived = false,
            rootPlaceId = uni.rootPlaceId,
            isActive = assetInfo.moderationStatus != ModerationStatus.Declined,
            privacyType = "Public",
            creatorType = assetInfo.creator.type,
            creatorTargetId = uni.creatorId,
            creatorName = uni.creatorName,
            created = uni.created,
            updated = uni.updated
        };
    }

    [HttpGet("universes/{universeId}/configuration")]    
    public async Task<dynamic> UniverseConfiguration(long universeId)
    {
        var uni = (await services.games.MultiGetUniverseInfo(new[] {universeId})).FirstOrDefault();
        var assetInfo = (await services.assets.MultiGetAssetDeveloperDetails(new[] {uni.rootPlaceId})).First();
        var details = await services.assets.GetAssetCatalogInfo(uni.rootPlaceId);
        List<long> playableDevices = new List<long>
        { 
            1,
        };
        return new UniverseConfiguration
        {
            allowPrivateServers = false,
            privateServerPrice = 0,
            id = universeId,
            name = uni.name,
            universeAvatarType = 1,
            universeScaleType = 1,
            universeAnimationType = 1,
            universeCollisionType = 1,
            universeBodyType = 1,
            universeJointPositioningType = 1,
            isArchived = false,
            isFriendsOnly = false,
            genre = assetInfo.genres,
            playableDevices = playableDevices,
            isForSale = details.isForSale,
            price = 0,
            isStudioAccessToApisAllowed = true,
            privacyType = privacyType.Public,
        };
    }
}