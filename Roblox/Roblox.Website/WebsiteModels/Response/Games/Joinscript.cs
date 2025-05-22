namespace Roblox.Website.WebsiteModels.Games;
public class BaseJoinScript
{
    public int ClientPort { get; set; }
    public string MachineAddress { get; set; }
    public int ServerPort { get; set; }
    public string PingUrl { get; set; }
    public int PingInterval { get; set; }
    public string UserName { get; set; }
    public bool SeleniumTestMode { get; set; }
    public int UserId { get; set; }
    public bool SuperSafeChat { get; set; }
    public string CharacterAppearance { get; set; }
    public string ClientTicket { get; set; }
    public int GameId { get; set; }
    public int PlaceId { get; set; }
    public string MeasurementUrl { get; set; }
    public string WaitingForCharacterGuid { get; set; }
    public string BaseUrl { get; set; }
    public string ChatStyle { get; set; }
    public int VendorId { get; set; }
    public string ScreenShotInfo { get; set; }
    public string VideoInfo { get; set; }
    public int CreatorId { get; set; }
    public string CreatorTypeEnum { get; set; }
    public string MembershipType { get; set; }
    public int AccountAge { get; set; }
    public string CookieStoreFirstTimePlayKey { get; set; }
    public string CookieStoreFiveMinutePlayKey { get; set; }
    public bool CookieStoreEnabled { get; set; }
    public bool IsRobloxPlace { get; set; }
    public bool GenerateTeleportJoin { get; set; }
    public bool IsUnknownOrUnder13 { get; set; }
    public string SessionId { get; set; }
    public int DataCenterId { get; set; }
    public int UniverseId { get; set; }
    public int BrowserTrackerId { get; set; }
    public bool UsePortraitMode { get; set; }
    public int FollowUserId { get; set; }
    public int CharacterAppearanceId { get; set; }
}