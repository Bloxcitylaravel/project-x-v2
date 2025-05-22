using MVC = Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class Locales: ControllerBase
    {
        [HttpGetBypass("v1/locales")]
        public dynamic GetLocales()
        {
            string localesRaw = System.IO.File.ReadAllText(Configuration.JsonDataDirectory + "Local.json");
            dynamic? local = JsonConvert.DeserializeObject<ExpandoObject>(localesRaw);
            return local ?? "";
        }
        [HttpGetBypass("v1/locales/user-localization-locus-supported-locales")]
        public dynamic GetLocalesOther()
        {
            string localesRaw = System.IO.File.ReadAllText(Configuration.JsonDataDirectory + "Supportedlocales.json");
            dynamic? local = JsonConvert.DeserializeObject<ExpandoObject>(localesRaw);
            return local ?? "";
        }
    }
}
