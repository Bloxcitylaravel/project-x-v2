using System.Net;
using Roblox.Models.GameServer;

namespace Roblox.Models.Games;
public class PlaceLaunchRequest
{
    public string request { get; set; }
    public long placeId { get; set; }
    public string? gameId { get; set; } = null;
    public bool isPartyLeader { get; set; }
    public bool isTeleport { get; set; }
    public string? accessCode { get; set; }
    public string? linkCode { get; set; }
    public string? privateGameMode { get; set; }
    public string? cookie { get; set; } 
    public string? username { get; set; } = null;
    public long? userId { get; set; } = null;
    public bool? special { get; set; } = false;
}

public class PlaceLaunchResponse
{
    public string? jobId { get; set; }
    public int status { get; set; }
    public string? joinScriptUrl { get; set; }
    public string? authenticationUrl { get; set; }
    public string? authenticationTicket { get; set; }
    public string? message { get; set; }
    public dynamic joinScript { get; set; }
}