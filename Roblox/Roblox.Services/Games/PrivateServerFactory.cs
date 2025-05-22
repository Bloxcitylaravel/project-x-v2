using System.Formats.Asn1;
using System.Security.Cryptography;
using Dapper;
using Roblox.Dto;
using Roblox.Dto.Games;
using Roblox.Libraries;
using Roblox.Models.Assets;
using Roblox.Services.Exceptions;
using Type = Roblox.Models.Assets.Type;

namespace Roblox.Services;
public enum PrivateServerType
{
    Active = 1,
    Inactive,
    Cancelled,
}
public class PrivateServerService : ServiceBase
{
    /*
    internal static string GenerateLinkCode(long privateServerId, IUser authenticatedUser)
    {
        FloodChecker floodChecker = GetPrivateServerUpdateLinkFloodChecker(privateServerId);
        Roblox.Platform.Games.Entities.PrivateServer privateServerEntity = FetchAndValidatePrivateServerEntity(privateServerId, authenticatedUser, floodChecker);
        if (privateServerEntity == null)
        {
            return null;
        }
        privateServerEntity.LinkCode = GenerateCryptographicallyStrongString(32);
        privateServerEntity.Save();
        floodChecker.UpdateCount();
        return privateServerEntity.LinkCode;
    }
    */

    private static string GenerateCryptographicallyStrongString(byte length)
    {
        int num = (byte)Math.Ceiling((double)(int)length / 4.0) * 3;
        byte[] randomBytes = new byte[num];
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes).Substring(0, length).Replace('+', '-')
            .Replace('/', '_');
    }
}
