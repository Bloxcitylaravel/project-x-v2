
using InfluxDB.Client.Core.Exceptions;
using Roblox;
using Roblox.Dto.Games;
using Roblox.Models.Games;
using Roblox.Models.GameServer;
using Roblox.Services;
using Roblox.Services.Signer;
namespace Roblox.Services.PlaceLauncher;
public class PlaceLauncherService : ServiceBase
{
    public enum MatchmakingContextId
    {
        Default = 1,
        Xbox,
        CloudEdit,
        CloudEditTest,
    }

    public async Task<PlaceLaunchResponse> PlaceLauncherAsync(PlaceLaunchRequest plRequest)
    {
        switch (plRequest.request)
        {
            case "RequestGameJob":
                if (plRequest.gameId == null)
                {
                    throw new BadRequestException("Game id is missing");
                };
                return await RequestGameJob(plRequest.gameId, plRequest.placeId);
            case "RequestGame":
                return await RequestGame(plRequest.placeId, (int)MatchmakingContextId.Default, plRequest.cookie, plRequest.special, plRequest.username, plRequest.userId);
            case "CloudEdit":
                return await RequestGame(plRequest.placeId, (int)MatchmakingContextId.CloudEdit, plRequest.cookie);
            case "RequestPrivateGame":
                break;
        }
        //default 
        return new PlaceLaunchResponse()
        {
            status = (int)JoinStatus.Error,
            message = "An error occured while starting the game."  
        };
    }

    public async Task<PlaceLaunchResponse> RequestGameJob(string gameId, long placeId)
    {
        GamesService games = new GamesService();
        
        return new PlaceLaunchResponse()
        {
            jobId = gameId,
            status = (int)JoinStatus.Joining,
            joinScriptUrl = $"{Roblox.Configuration.BaseUrl}/Game/Join.ashx?jobId={gameId}&placeId={placeId}",
            authenticationUrl = $"{Roblox.Configuration.BaseUrl}/Login/Negotiate.ashx",
            authenticationTicket = "hi",
            message = (string)null,
        };
    }
    public async Task<PlaceLaunchResponse> RequestGame(long placeId, int matchmaking, string cookie, bool? Special = false, string? username = null, long? userId = null)
    {
        GamesService games = new GamesService();
        GameServerService gameServer = new GameServerService();
        UsersService users = new UsersService();
        SignService sign = new SignService();
        var result = await gameServer.GetServerForPlace(placeId, matchmaking);
        dynamic joinScript = null;
        string finalTicket;
        if ((bool)Special)
        {
            var jobPlayers = await gameServer.GetGameServerPlayers(result.job);
            PlaceEntry uni = (await games.MultiGetPlaceDetails(new[] { placeId })).First();
            long year = await games.GetYear(placeId);
            string membership;
            var membership2 = await users.GetUserMembership((long)userId);
            DateTime currentUtcDateTime = DateTime.UtcNow;
            string formattedDateTime = currentUtcDateTime.ToString("M/d/yyyy h:mm:ss tt");
            var userInfo = await users.GetUserById((long)userId);
            var accountAgeDays = DateTime.UtcNow.Subtract(userInfo.created).Days;
            if (membership2 == null)
            {
                membership = "None";
            }
            else
            {
                membership = (int)membership2!.membershipType == 4 ? "Premium" : (int)membership2!.membershipType == 3 ? "OutrageousBuildersClub" : (int)membership2.membershipType == 2 ? "TurboBuildersClub" : (int)membership2.membershipType == 1 ? "BuildersClub" : "None";
            }
            string characterAppearanceUrl = $"{Configuration.BaseUrl}/v1/avatar-fetch?userId={userId}&placeId={placeId}";
            finalTicket = sign.GenerateClientTicketV4((long)userId, username, characterAppearanceUrl, membership, result.job, formattedDateTime, accountAgeDays, placeId);
            joinScript = await games.GetJoinScript(year, username, (long)userId, result.job, placeId, uni.universeId, uni.builderId, characterAppearanceUrl, finalTicket, membership, accountAgeDays, true, cookie);
        }

        if (result.status == JoinStatus.Joining)
        {
            await Roblox.Metrics.GameMetrics.ReportGameJoinPlaceLauncherReturned(placeId);

            return new PlaceLaunchResponse()
            {
                jobId = result.job,
                status = (int)result.status,
                joinScriptUrl = $"{Roblox.Configuration.BaseUrl}/Game/Join.ashx?jobId={result.job}&placeId={placeId}",
                authenticationUrl = Roblox.Configuration.BaseUrl + "/Login/Negotiate.ashx",
                authenticationTicket = cookie,
                message = (string?)null,
                joinScript = (bool)Special ? joinScript : null 
            };
        }
        return new PlaceLaunchResponse()
        {
            jobId = (string?)null,
            status = (int)JoinStatus.Loading,
            message = "Server found, loading...",
        };
    }
}