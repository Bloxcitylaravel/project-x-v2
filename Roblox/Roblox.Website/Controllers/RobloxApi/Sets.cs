using MVC = Microsoft.AspNetCore.Mvc;
using CsvHelper;
using System.Xml;
namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class Sets: ControllerBase
    {
        [HttpGetBypass("Game/Tools/InsertAsset.ashx")]
        public async Task<dynamic> InsertAsset(long? sid, long? nsets, string? type, long? userId)
        {
            string setData = await services.sets.GrabSet(sid, nsets, type, userId);
            if (setData == null)
            {
                return BadRequest();
            }
            return Content(setData, "text/xml");
        }
    }
}