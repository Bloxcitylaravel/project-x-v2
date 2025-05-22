namespace Roblox.Models.Studio;
public class Universe
{
    public int id { get; set; }
    public string name { get; set; }
    public object description { get; set; }
    public bool isArchived { get; set; }
    public int rootPlaceId { get; set; }
    public bool isActive { get; set; }
    public string privacyType { get; set; }
    public string creatorType { get; set; }
    public int creatorTargetId { get; set; }
    public string creatorName { get; set; }
    public DateTime created { get; set; }
    public DateTime updated { get; set; }
}
public class UpdatePlace
{
    public long id { get; set; }
    public long universeid { get; set; }
}