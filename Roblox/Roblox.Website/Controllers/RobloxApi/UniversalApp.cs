using MVC = Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;
namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class UniversalAppConfiguration: ControllerBase
    {
        [HttpGetBypass("universal-app-configuration/v1/behaviors/app-patch/content")]
        public dynamic AppPatch()
        {
            List<long> CanaryUserIds = new List<long>();
            return new 
            {
                SchemeVersion = "1",
                CanaryUserIds,
                CanaryPercentage = 0,
            };
        }


        [HttpGetBypass("universal-app-configuration/v1/behaviors/app-policy/content")]
        public dynamic AppPolicy()
        {
            string policyContent = System.IO.File.ReadAllText(Configuration.JsonDataDirectory + "AppPolicy.json");
            dynamic? policyJson = JsonConvert.DeserializeObject<ExpandoObject>(policyContent);
            return policyJson ?? "";
        }
    }
}