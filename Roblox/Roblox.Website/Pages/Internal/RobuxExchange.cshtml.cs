using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Models.Users;
using System.Threading.Tasks;

namespace Roblox.Website.Pages.Internal
{
    public class RobuxExchange : RobloxPageModel
    {
        [BindProperty]
        public string? successMessage { get; set; }
        [BindProperty]
        public string? errorMessage { get; set; }
        [BindProperty]
        public long robux { get; set; }

        public void OnGet()
        {
            if (userSession == null)
            {
                return;
            }
        }

        public async Task OnPost()
        {
            if (userSession == null)
            {
                return;
            }

            if (robux < 10)
            {
                errorMessage = "The minimum tix you can exchange is 10.";
                return;
            }

            int conversionRate = 10;
            decimal roughTix = robux * conversionRate;
            long finaltix = (long)Math.Round(roughTix, 0);

            try
            {
                var balance = await services.economy.GetUserBalance(userSession.userId);
                long newBalance = balance.robux;

                if (newBalance < robux)
                {
                    errorMessage = "Insufficient robux balance.";
                    return;
                }

                await services.economy.ChargeForConversion(userSession.userId, robux, finaltix, Roblox.Models.Economy.ConversionType.RobuxToTix);

                successMessage = $"You have received {finaltix} tickets from {robux} R$.";
                return;
            }
            catch (Exception)
            {
                errorMessage = "Failed to convert tix to robux.";
                return;
            }
        }
    }
}
