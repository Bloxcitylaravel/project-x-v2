using MVC = Microsoft.AspNetCore.Mvc;

using Dapper;
using Npgsql;
using Roblox.Services.Exceptions;
namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class UserInfoBot: ControllerBase
    {
        private NpgsqlConnection db => services.assets.db;
        
        [BotAuthorization]
        [HttpGetBypass("bot/balance")]
        public async Task<object> GetUserBalanceAsync(string discordid)
        {

            Dto.Users.UserInfo userInfo;
            try
            {
                userInfo = await services.users.GetUserByDiscordId(discordid);
            }
            catch (RecordNotFoundException)
            {
                return new
                {
                    message = "User not found"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new
                {
                    message = "An error occurred while retrieving user information"
                };
            }

            var bal = await services.economy.GetUserBalance(userInfo.userId);

            return new
            {
                bal.tickets,
                bal.robux
            };
        }
    }
}