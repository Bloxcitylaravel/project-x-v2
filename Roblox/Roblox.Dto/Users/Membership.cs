using Roblox.Models.Users;

namespace Roblox.Dto.Users;

public class UserMembershipEntry
{
    public long userId { get; set; }
    public MembershipType membershipType { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime updatedAt { get; set; }
}

public class MembershipMetadata
{
    private static List<MembershipMetadata> _membershipMetadata = new()
    {
        new MembershipMetadata(MembershipType.None, "None", 0),
        new MembershipMetadata(MembershipType.BuildersClub, "Builders Club", 15),
        new MembershipMetadata(MembershipType.TurboBuildersClub, "Turbo Builders Club", 35),
        new MembershipMetadata(MembershipType.OutrageousBuildersClub, "Outrageous Builders Club", 85),
        new MembershipMetadata(MembershipType.Premium, "Premium", 85),
    };

    public static MembershipMetadata GetMetadata(MembershipType membershipType)
    {
        var result = _membershipMetadata.Find(v => v.membershipType == membershipType);
        if (result == null)
            throw new ArgumentException("Invalid " + nameof(membershipType));
        return result;
    }

    public MembershipMetadata(MembershipType type, string displayName, long dailyRobux)
    {
        this.membershipType = type;
        this.dailyRobux = dailyRobux;
        this.displayName = displayName;
    }
    
    public long dailyRobux { get; set; }
    public MembershipType membershipType { get; set; }
    public string displayName { get; set; }
}