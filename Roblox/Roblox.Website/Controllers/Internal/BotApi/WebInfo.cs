using MVC = Microsoft.AspNetCore.Mvc;

using Dapper;
using Npgsql;
namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class WebInfo: ControllerBase
    {
        private NpgsqlConnection db => services.assets.db;
        
        [BotAuthorization]
        [HttpGetBypass("bot/status")]
        public async Task<dynamic> GetWebInfo()
        {
            var t = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(60));
            var OnlineCountQuery = await db.QuerySingleOrDefaultAsync("SELECT COUNT(*) as total FROM \"user\" WHERE online_at >= :t", new
            {
                t,
            });
            var IngameQuery = await db.QuerySingleOrDefaultAsync("SELECT COUNT(*) as total FROM asset_server_player", new
            {
                t,
            });

            long OnlineCount = OnlineCountQuery?.total ?? 0;
            long Ingame = IngameQuery?.total ?? 0;
            
            return new 
            {
                Online = OnlineCount,
                Ingame = Ingame
            };
        }
    }
}