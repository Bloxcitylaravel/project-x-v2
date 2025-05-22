using MVC = Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;
namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class FeatureFlagsRoblox: ControllerBase
    {
        private static readonly HashSet<string> AllowedTypes = new HashSet<string>
        {
            "iOSAppSettings",
            "AndroidAppSettings",
            "StudioAppSettings"
        };
        [HttpPostBypass("Setting/Get/{type}")]
        [HttpPostBypass("Setting/QuietGet/{type}")]
        [HttpGetBypass("Setting/Get/{type}")]
        [HttpGetBypass("Setting/QuietGet/{type}")]
        public ActionResult<dynamic> GetAppSettings(string type, string apiKey)
        {
            bool isValid = true;
            
            switch (apiKey)
            {
                case "C1273ADA-5726-46D7-BA0C-D339228C697D"://2015 RCC 
                    type = "RCCService2015";
                    break;
                case "4C3DEC7F-7725-498F-BCA7-6389ED71E248": //2015 Client
                    type = "AppSettingsMulti2015";
                    break;
                case "9CE2063F-BB45-449B-89D4-65CD2ED806CD":  //2017L RCC
                    type = "RCCServiceUJ38BA31M8F47VA76XZ1RYONSSTILA3F";
                    break;
                case "08BF6621-8100-4484-B14C-87497E372160": //2017L Studio + Client
                    if(type == "StudioAppSettings")
                        break;
                    type = "ClientAppSettings2017";
                    break;
                case "D6925E56-BFB9-4908-AAA2-A5B1EC4B2D7A":  //2018L RCC
                    type = "RCCService2018";
                    break;
                case "76E5A40C-3AE1-4028-9F10-7C62520BD94F":
                case "19C0B314-AC23-4CD4-8A37-02C4140F7240":  ///2018L AppSettings
                    type = "ClientAppSettings2018";
                    break;
                default:
                //this is for 2016 temmporary lmao
                    isValid = AllowedTypes.Contains(type);
                    if (!isValid) {
                        type = "ClientAppSettings";
                    }
                    break;
            }
            
            try
            {
                string FFlag = Path.Combine(Configuration.JsonDataDirectory, $"{type}.json");
                if (!System.IO.File.Exists(FFlag)) return NotFound();
                
                string jsonContent = System.IO.File.ReadAllText(FFlag);
                dynamic? clientAppSettingsData = JsonConvert.DeserializeObject<ExpandoObject>(jsonContent);
                return clientAppSettingsData ?? new ExpandoObject();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RetrieveClientFFlags] Error while retrieving FFlags: {ex.Message}");
                return BadRequest("Error fetching FFlags");
            }
        }
        [HttpPostBypass("v2/settings/application")]
        [HttpGetBypass("v2/settings/application")]
        [HttpPostBypass("v1/settings/application")]
        [HttpGetBypass("v1/settings/application")]
        public MVC.ActionResult<dynamic> GetAppSettingsNew(string applicationName)
        {
            string realApp;
            switch(applicationName)
            {
                case "RCCService2019":
                    realApp = "RCCService2019";
                    break;
                case "PCDesktopClient2019":
                    realApp = "PCDesktopClient2019";
                    break;
                case "RCCService2020":
                    realApp = "RCCService2020";
                    break;
                case "PCStudioApp":
                    realApp = "StudioApp";
                    break;
                case "PCStudio221":
                    realApp = "Studio221";
                    break;
                case "RCCService2021":
                    realApp = "RCCService2021";
                    break;
                case "PCDesktopClient":
                    realApp = "PCDesktopClient";
                    break;
                case "PCDesktopClient2021":
                    realApp = "PCDesktopClient2021";
                    break;
                case "AndroidApp":
                    realApp = "AndroidApp";
                    break;
                case "iOSApp":
                    realApp = "iOSApp";
                    break;
                default:
                    return NotFound();
            }
            string sanatized = Path.GetFileName(realApp);
            if(sanatized == null)
                return NotFound();

            string jsonFilePath = Path.Combine(Configuration.JsonDataDirectory, sanatized + ".json");
            string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
            dynamic? clientAppSettingsData = JsonConvert.DeserializeObject<ExpandoObject>(jsonContent);

            return clientAppSettingsData ?? "";
        }
    }
}