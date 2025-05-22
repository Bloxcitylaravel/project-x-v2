using System.Text.Json.Serialization;
using Roblox.Models.Assets;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Dto.Games;

public class UniverseCreator
{
    public long id { get; set; }
    public string name { get; set; }
    public CreatorType type { get; set; }
    public bool isRNVAccount { get; set; } = false;
    public bool hasVerifiedBadge { get; set; } = false;
    
}

public class MultiGetUniverseEntry
{
    public long id { get; set; }
    public long rootPlaceId { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public string sourceName { get; set; } 
    public string? sourceDescription { get; set; }
    public Genre genre { get; set; }

    public UniverseCreator creator => new UniverseCreator()
    {
        id = creatorId,
        type = creatorType,
        name = creatorName
    };
    public long favoritedCount { get; set; }
    public bool isFavoritedByUser { get; set; }
    public bool isAllGenre => genre == Genre.All;
    public string universeAvatarType { get; set; } = "MorphToR6";
    public bool studioAccessToApisAllowed { get; set; }
    public long? price { get; set; }
    public bool isGenreEnforced { get; set; } = false;
    public long playing { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
    public int maxPlayers { get; set; }
    public long visits { get; set; }
    public bool createVipServersAllowed { get; set; }
    [JsonIgnore]
    public long creatorId { get; set; }
    [JsonIgnore]
    public CreatorType creatorType { get; set; }
    [JsonIgnore]
    public string creatorName { get; set; }
}

public class GameListEntry
{
    public long universeId { get; set; }
    public string name { get; set; }
    public long placeId { get; set; }
    public string gameDescription { get; set; }
    public int playerCount { get; set; }
    public long visitCount { get; set; }
    public long creatorId { get; set; }
    public CreatorType creatorType { get; set; }
    public string creatorName { get; set; }
    public Genre genre { get; set; }
    public int totalUpVotes { get; set; }
    public int totalDownVotes { get; set; }
    public string? analyticsIdentifier { get; set; }
    public long? price { get; set; }
    public bool isShowSponsoredLabel { get; set; }
    public string nativeAdData { get; set; } = "";
    public bool isSponsored { get; set; }
    public long year { get; set; }
    public string imageToken => "T_" + placeId + "_icon";
}
public class GameListEntryRoblox
{
    public long CreatorID { get; set; }
    public string CreatorName { get; set; }
    public string CreatorUrl = $"https://www.projex.zip/users/1/profile";
    public long Plays { get; set; }
    public int Price  { get; set; }
    public int ProductID = 0;
    public bool IsOwned = false;
    public bool IsVotingEnabled = true;
    public int TotalUpVotes { get; set; }
    public int TotalDownVotes { get; set; }
    public long TotalBought = 1;
    public long UniverseID { get; set; }
    public bool HasErrorOcurred = false;
    public string GameDetailReferralUrl = Roblox.Configuration.BaseUrl;
    public string Url = "";
    public string RetryUrl = null;
    public bool Final = true;
    public string Name { get; set; }
    public long PlaceID { get; set; }
    public long PlayerCount { get; set; }
    public long ImageId = 2311;
}
public class GamesForCreatorEntryDb
{
    public long id { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public long rootAssetId { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
    public long visitCount { get; set; }
}

public class RootPlaceEntry
{
    public long id { get; set; }
    public Type type => Type.Place;
}
public class GamesForCreatorDevelop
{
    public long id { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public bool isArchived { get; set;} = false;
    public long rootPlaceId { get; set; }
    public bool isActive { get; set;} = true;
    public string privacyType { get; set;} = "Public";
    public int creatorType { get; set; }
    public long creatorTargetId { get; set;}
    public string creatorName { get; set;}
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
}
public enum privacyType
{
    Public = 1,
    Private
}
public class UniverseConfiguration
{



    public bool allowPrivateServers { get; set;}
    public long privateServerPrice { get; set;}
    public bool isMeshTextureApiAccessAllowed { get; set;}
    public long id { get; set; }
    public string name { get; set; }
    public long universeAvatarType { get; set; }
    public long universeScaleType { get; set; }
    public long universeAnimationType { get; set; }
    public long universeCollisionType { get; set; }
    public long universeBodyType { get; set; }
    public long universeJointPositioningType { get; set; }
    public bool isArchived { get; set; }
    public bool isFriendsOnly { get; set; }
    public IEnumerable<Genre> genre { get; set; }
    public List<long> playableDevices { get; set; }
    public bool isForSale { get; set; }
    public int price { get; set; }
    public bool isStudioAccessToApisAllowed { get; set; }
    public privacyType privacyType { get; set;}
}
public class GamesForCreatorEntry
{
    public long id { get; set; }
    public string name { get; set; }
    public string? description { get; set; }
    public long rootPlaceId { get; set;}
    public RootPlaceEntry rootPlace { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
    public long placeVisits { get; set; }
}

public class CreateUniverseResponse
{
    public long universeId { get; set; }
}

public class PlayEntry
{
    public DateTime createdAt { get; set; }
    public DateTime? endedAt { get; set; }
    public long placeId { get; set; }
}
public class SetYearRequest
{
    public int year { get; set; }
}
public class SetMaxPlayerCountRequest
{
    public int maxPlayers { get; set; }
}