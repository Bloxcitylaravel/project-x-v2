using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Roblox.Services;
using Roblox.Website.WebsiteModels;
using Roblox.Dto.Avatar;
using Roblox.Exceptions;
using Roblox.Models.Avatar;
using Roblox.Services.App.FeatureFlags;
using ServiceProvider = Roblox.Services.ServiceProvider;
#pragma warning disable CS8600
namespace Roblox.Website.Controllers;

[Route("/")]
public class Voice : ControllerBase
{
    [HttpGetBypass("v1/settings")]
    public dynamic VoiceSettingsGlobal()
    {
        return new
        {
            isVoiceEnabled = true,
            isUserOptIn = true,
            isUserEligible = true,
            isBanned = false,
            banReason = 0,
            bannedUntil = (object)null,
            canVerifyAgeForVoice = true,
            isVerifiedForVoice = true,
            denialReason = 0,
            isOptInDisabled = false,
            hasEverOpted = true,
            isAvatarVideoEnabled = true,
            isAvatarVideoOptIn = true,
            isAvatarVideoOptInDisabled = true,
            isAvatarVideoEligible = true,
            hasEverOptedAvatarVideo = true,
            userHasAvatarCameraAlwaysAvailable = false,
            canVerifyPhoneForVoice = false,
            seamlessVoiceStatus = 1,
            allowVoiceDataUsage = true
        };

    }

    [HttpGetBypass("v1/settings/universe/{universeId:long}")]
    public dynamic VoiceSettingsUniverse(long universeId)
    {
        return new
        {
            isUniverseEnabledForVoice = true,
            isPlaceEnabledForVoice = true,
            isUniverseEnabledForAvatarVideo = true,
            isPlaceEnabledForAvatarVideo = true
        };
    }
}