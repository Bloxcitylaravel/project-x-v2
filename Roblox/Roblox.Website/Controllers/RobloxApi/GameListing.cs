using MVC = Microsoft.AspNetCore.Mvc;
using CsvHelper;
using System.Xml;
using Roblox.Services.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Roblox.Dto.Marketplace;
using Newtonsoft.Json;
using System.Dynamic;
using Roblox.Models;
using Roblox.Dto.Friends;
using Roblox.Models.Assets;
using System.Text.RegularExpressions;
using Roblox.Dto.Games;
using System.ComponentModel.DataAnnotations;
namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class GameListing: ControllerBase
    {
        private static Regex numberRegex { get; } = new("([0-9]+)");
        [HttpGetBypass("v1/games/multiget-playability-status")]
        public dynamic MultiGetPlayabilityStatus()
        {
            var ids = HttpContext.Request.QueryString.Value;
            return numberRegex.Matches(ids).Select(c => long.Parse(c.Value)).Distinct().Select(c => new
            {
                playabilityStatus = 0,
                isPlayable = true,
                universeId = c,
            });
        }
        [HttpGetBypass("v1/games")]
        public async Task<dynamic> MultiGetUniverseInfo(string universeIds)
        {
            var sp = universeIds.Split(",").Select(long.Parse);
            var result = await services.games.MultiGetUniverseInfo(sp);
            return new
            {
                data = result,
            };
        }
        [HttpGetBypass("v1/games/sorts")]
        public dynamic GameSort()
        {
            string rawJson = System.IO.File.ReadAllText(Configuration.JsonDataDirectory + "GameSort.json");
            dynamic? json = JsonConvert.DeserializeObject<ExpandoObject>(rawJson);
            return json ?? new ExpandoObject();
        }
        [HttpGetBypass("v1/name-description/games/{universeId:long}")]
        public async Task<dynamic> GetGameDesc(long universeId)
        {
            var uni = (await services.games.MultiGetUniverseInfo(new[] {universeId})).FirstOrDefault();
            return new
            {
                data = new[]
                {
                    new
                    {
                        name = uni.name,
                        description = uni.description,
                        languageCode = "en"
                    }
                }
            };
        }
        [HttpGetBypass("v1/games/{universeId:long}/social-links/list")]
        public dynamic GetSocialLinks()
        {
            return new
            {
                data = new List<int>(),
            };
        }

        [HttpGetBypass("/v1/games/{universeId:long}/favorites")]
        public async Task<dynamic> GetFavoriteStatus(long universeId)
        {
            return new 
            {
                isFavorited = await services.assets.GetFavoriteStatus(safeUserSession.userId, universeId)
            };
        }
        [HttpGetBypass("v1/universes/{universeId:long}/badges")]
        [HttpGetBypass("v1/games/{universeId:long}/game-passes")]
        public async Task<dynamic> GetGameGamePasses(long universeId, int maxRows = 6)
        {
            return new
            {
                previousPageCursor = (string?) null,
                nextPageCursor = (string?) null,
                data = new List<object>()
            };
        }
        [HttpGetBypass("v1/games/recommendations/game/{universeId:long}")]
        public async Task<dynamic> GetRecommendedGames(long universeId, int maxRows = 6)
        {
            if (maxRows is > 100 or < 1) maxRows = 10;
            // todo: actually add recommendeds
            var result = await services.games.GetGamesList(userSession.userId, "popular", maxRows, null, null);
            return new
            {
                games = result,
            };
        }

        [HttpGetBypass("v1/games/multiget-place-details")]
        public async Task<IEnumerable<PlaceEntry>> MultiGetPlaceDetails(string placeIds)
        {
            return await services.games.MultiGetPlaceDetails(placeIds.Split(",").Select(long.Parse));
        }
        [HttpGetBypass("v1/games/votes")]
        public async Task<dynamic> GetGameVotes(string universeIds)
        {
            var ids = universeIds.Split(",").Select(long.Parse).Distinct().ToList();
            if (ids.Count is < 1 or > 100)
                throw new RobloxException(400, 0, "BadRequest");
            var uni = await services.games.MultiGetUniverseInfo(ids);

            var result = new List<dynamic>();
            foreach (var item in uni)
            {
                var votes = await services.assets.GetVoteForAsset(item.rootPlaceId);
                result.Add(new
                {
                    id = item.id,
                    upVotes = votes.upVotes,
                    downVotes = votes.downVotes,
                });
            }

            return new
            {
                data = result,
            };
        }

        [HttpPatch("v1/games/{universeId:long}/user-votes")]
        public async Task VoteOnUniverse(long universeId, [Required, FromBody] VoteRequest request)
        {
            var uni = (await services.games.MultiGetUniverseInfo(new[] {universeId})).FirstOrDefault();
            await services.assets.VoteOnAsset(uni.rootPlaceId, safeUserSession.userId, request.vote);
        }
        [HttpGetBypass("v1/games/list")]
        public async Task<dynamic> GetGamesList(string? sortToken, int maxRows = 10, Genre? genre = null, string? keyword = null)
        {
            if (maxRows is > 100 or < 1) maxRows = 10;
            var result = await services.games.GetGamesList(userSession?.userId, sortToken, maxRows, genre, keyword);
            return new
            {
                games = result,
            };
        }
        [HttpGetBypass("v2/users/{userId:long}/games")]
        public async Task<RobloxCollectionPaginated<GamesForCreatorEntry>> GetUserGames(long userId,
            string? sortOrder, string? accessFilter, int limit, string? cursor = null)
        {
            if (limit is > 100 or < 1) limit = 10;
            int offset = int.Parse(cursor ?? "0");
            var result =
                (await services.games.GetGamesForType(CreatorType.User, userId, limit, offset, sortOrder ?? "asc", accessFilter ?? "All")).ToList();
            return new RobloxCollectionPaginated<GamesForCreatorEntry>()
            {
                nextPageCursor = result.Count >= limit ? (offset+limit).ToString(): null,
                previousPageCursor = offset >= limit ? (offset-limit).ToString() : null,
                data = result,
            };
        }
        
        [HttpGetBypass("v2/groups/{groupId:long}/games")]
        public async Task<RobloxCollectionPaginated<GamesForCreatorEntry>> GetGroupGames(long groupId,
            string? sortOrder, string? accessFilter, int limit, string? cursor = null)
        {
            if (limit is > 100 or < 1) limit = 10;
            int offset = int.Parse(cursor ?? "0");
            var result =
                (await services.games.GetGamesForType(CreatorType.Group, groupId, limit, offset, sortOrder, accessFilter)).ToList();
            return new RobloxCollectionPaginated<GamesForCreatorEntry>()
            {
                nextPageCursor = result.Count >= limit ? (offset+limit).ToString(): null,
                previousPageCursor = offset >= limit ? (offset-limit).ToString() : null,
                data = result,
            };
        }

        /// <summary>
        /// Endpoint is only valid for custom media (such as videos or custom thumbnails. Auto generated and/or default thumbnails are not returned.
        /// </summary>
        [HttpGetBypass("v2/games/{universeId}/media")]
        public async Task<RobloxCollection<GameMediaEntry>> GetGameMedia(long universeId)
        {
            var place = await services.games.MultiGetUniverseInfo(new[] {universeId});
            var result = await services.games.GetGameMedia(place.First().rootPlaceId);
            return new()
            {
                data = result,
            };
        }
    }
}