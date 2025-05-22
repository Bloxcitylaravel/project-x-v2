using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Exceptions;
using Roblox.Libraries.Captcha;
using Roblox.Logging;
using Roblox.Metrics;
using Roblox.Models.Users;
using Roblox.Services;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;
using Roblox.Website.Controllers;
using ControllerBase = Microsoft.AspNetCore.Mvc.ControllerBase;

namespace Roblox.Website.Pages.Auth;

public class TotpSetup : RobloxPageModel
{
    [BindProperty]
    public string? totpcode { get; set; }
    [BindProperty]
    public string? mode { get; set; }
    public string? errorMessage { get; set; }
    public string? successMessage { get; set; }
    public string? secret { get; set; }
    public string? qrcode { get; set; }
    public int? status { get; set; }
    public async Task OnGet()
    {
        //auth check!!!!
        if (userSession == null) {
            throw new UnauthorizedException();
        }
        //get the totpinfo
        TotpInfo totpInfo = await services.users.GetOrSetTotp(userSession.userId);
        //set secret and qrcode
        secret = totpInfo.secret;
        qrcode = services.users.GetOtpQrCodeBase64(userSession.userId, totpInfo.secret);
        status = (int)totpInfo.status;
        if (totpInfo.status == TotpStatus.Enabled) {
            successMessage = "2FA is already enabled";
        }
    }
    
    public async Task<IActionResult> OnPost()
    {
        long code = 0;
        // null checks
        if (userSession == null) {
            throw new UnauthorizedException();
        }

        //get totpinfo
        TotpInfo totpInfo = await services.users.GetOrSetTotp(userSession.userId);
        //set secret and qrcode
        secret = totpInfo.secret;

        if (totpcode == null || string.IsNullOrWhiteSpace(totpcode)) {
            errorMessage = "You must provide a 2FA Code";
            return new PageResult();
        }

        //parse the totp code as a long
        if (!long.TryParse(totpcode, out code)) {
            errorMessage = "The 2FA code you entered is not valid";
            return new PageResult();
        }

        //verify the totp code
        //if it is valid, set the success message and update the user's totp status to enabled
        if (services.users.VerifyTotp(secret, totpcode)) {
            if (!string.IsNullOrEmpty(mode) && mode == "delete") {
                await services.users.DeleteTotp(userSession.userId);
                status = (int)TotpStatus.Disabled;
                successMessage = "2FA has been disabled";
                return new PageResult();
            }
            await services.users.UpdateTotpStatus(userSession.userId, TotpStatus.Enabled);
            status = (int)TotpStatus.Enabled;
            successMessage = "2FA has been enabled";
            return new PageResult();
        }

        //wrong code!!!!
        errorMessage = "You have entered a wrong code, please try again";
        return new PageResult();
    }
}