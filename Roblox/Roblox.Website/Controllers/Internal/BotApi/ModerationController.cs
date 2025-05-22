using MVC = Microsoft.AspNetCore.Mvc;

namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class ModerationBot: ControllerBase
    {
        [BotAuthorization]
        [HttpGetBypass("bot/kickuser")]
        public async Task<MVC.IActionResult> KickPlayerFromBot(long userId)
        {
            await services.gameServer.KickPlayer(userId);
            return Ok();
        } 
    }
}
