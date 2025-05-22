using Dapper;
using Roblox.Dto;
using Roblox.Dto.Friends;
using Roblox.Dto.Users;
using Roblox.Libraries.Cursor;
using Roblox.Metrics;
using Roblox.Models;
using Roblox.Models.Users;
using Roblox.Services.Exceptions;

namespace Roblox.Services;

public class HWID : ServiceBase, IService
{
    public async Task<bool> CheckDuplicateHWID(long user_id, string HWID)
    {
        var isDuplicate = await db.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM hwid WHERE hwid.user_id = @user_id AND hwid = @HWID",
            new { user_id, HWID }
        );
        if(isDuplicate == 0)
        {
            return false;
        }
        return true;
    }


    public async Task AddHWID(long user_id, string HWID)
    {
        await db.ExecuteAsync(
            "INSERT INTO HWID (user_id, HWID) VALUES (@user_id, @HWID)",
            new { user_id, HWID }
        );
        return;
    }
    public async Task<bool> GetHWIDStatus(string HWID)
    {
        var isBanned = await db.QueryFirstOrDefaultAsync<bool>(
            "SELECT is_banned FROM HWID WHERE HWID = @HWID",
            new { HWID }
        );
        return isBanned;
    }    

    public async Task<bool> CheckHWID(long userId, string HWID)
    {
        bool CheckExist = await CheckDuplicateHWID(userId, HWID);
        bool status = await GetHWIDStatus(HWID);
        if(CheckExist){
            Console.WriteLine($"HWID Is already in the database {HWID}");
            switch (status){
            case true:
                Console.WriteLine("User is banned");
                return false;
            case false:
                Console.WriteLine("User is OK");
                return true;
            }
        }
        
        else{
            await AddHWID(userId, HWID);
            Console.WriteLine("Added HWID to database");
        }
        
        return true;
    }
    public bool IsThreadSafe()
    {
        return true;
    }
    public bool IsReusable()
    {
        return false;
    }
}