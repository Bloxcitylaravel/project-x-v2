using MVC = Microsoft.AspNetCore.Mvc;

namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class DiscordLink: ControllerBase
    {
        [BotAuthorization]
        [HttpGetBypass("bot/generatecode")]
        public async Task<dynamic> GenerateLinkCode(string discordId)
        {
            string authCode = await services.users.GenerateAuthCode(discordId);
            return authCode;
        }
        
        [HttpGetBypass("bot/verify")]
        public async Task<dynamic> LinkDiscord(string linkcode)
        {
            try
            {
                await services.users.LinkDiscordAccount(linkcode, safeUserSession.userId);
            }
            catch(Exception e)
            {
                Console.WriteLine($"Something went wrong while linking the account{e.Message.ToString()}");
                return "Something went wrong while trying to link your account";
            }
            return "Successfully linked your account";
        }
    }
}