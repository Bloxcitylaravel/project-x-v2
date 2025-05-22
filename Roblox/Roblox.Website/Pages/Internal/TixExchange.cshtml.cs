using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Models.Users;
using Roblox.Website.Controllers;
using System.Threading.Tasks;

namespace Roblox.Website.Pages.Internal
{
    public class TixExchange : RobloxPageModel
    {
        [BindProperty]
        public string? successMessage { get; set; }
        [BindProperty]
        public string? errorMessage { get; set; }
        [BindProperty]
        public long tix { get; set; }

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

            if (tix < 10)
            {
                errorMessage = "The minimum tix you can exchange is 10.";
                return;
            }

            int conversionRate = 10;
            decimal roughRobux = tix / conversionRate;
            long finalRobux = (long)Math.Round(roughRobux, 0);

            try
            {
                var balance = await services.economy.GetUserBalance(userSession.userId);
                long newBalance = balance.tickets;

                if (newBalance < tix)
                {
                    errorMessage = "Insufficient tix balance.";
                    return;
                }

                await services.economy.ChargeForConversion(userSession.userId, tix, finalRobux, Roblox.Models.Economy.ConversionType.TixToRobux);

                successMessage = $"You have received {finalRobux} R$ from {tix} Tix.";
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
