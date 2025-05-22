using MVC = Microsoft.AspNetCore.Mvc;
using CsvHelper;
using System.Xml;
namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class SiteAlertMobile: ControllerBase
    {
        [HttpGetBypass("alerts/alert-info")]
        public async Task<dynamic> GetAlert()
        {
            var alert = await services.users.GetGlobalAlert();
            return new
            {
                IsVisible = alert != null,
                Text = alert?.message ?? "",
                LinkText = "",
                LinkUrl = alert?.url ?? "",
            };
        }
    }
}