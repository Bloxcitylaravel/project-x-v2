
using MVC = Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
namespace Roblox.Website.Controllers
{

    [MVC.ApiController]
    [MVC.Route("/")]
    public class Telementry : ControllerBase
    {
        [HttpGetBypass("client/pbe")]
        [HttpGetBypass("mobile/pbe")]
        public OkResult PBE()
        {
            return Ok();
        }

        [HttpGetBypass("v1/enrollments")]
        [HttpPostBypass("v1/enrollments")]
        public dynamic Enrollments()
        {
            return new 
            {
                data = new[]
                {
                    new
                    {
                        SubjectType = "BrowserTracker",
                        SubjectTargetId = 63713166375,
                        ExperimentName = "AllUsers.DevelopSplashScreen.GreenStartCreatingButton",
                        Status = "Inactive",
                        Variation = (string)null
                    }
                }
            };
        }

        [HttpGetBypass("v1/get-enrollments")]
        [HttpPostBypass("v1/get-enrollments")]
        public dynamic GetEnrollments()
        {
            return Array.Empty<object>();
        }
    }
}