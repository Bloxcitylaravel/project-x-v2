using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Roblox.Dto.Users;
using Roblox.Services.App.FeatureFlags;
using Roblox.Services.Exceptions;

namespace Roblox.Website.Pages;

public class Upload : RobloxPageModel
{
    public long userId { get; set; }
    public void OnGet()
    {
        userId = userSession.userId;
    }
}