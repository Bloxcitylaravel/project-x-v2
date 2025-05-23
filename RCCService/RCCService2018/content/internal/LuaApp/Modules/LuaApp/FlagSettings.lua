local NotificationService = game:GetService("NotificationService")

local FlagSettings = {
	isLuaAppStarterScriptEnabled = false,
}

local function IsRunningInStudio()
	return game:GetService("RunService"):IsStudio()
end

-- Intended to be used by LuaAppStarterScript.lua only.
function FlagSettings:SetIsLuaAppStarterScriptEnabled(isEnabled)
	self.isLuaAppStarterScriptEnabled = isEnabled
end

function FlagSettings:IsLuaAppStarterScriptEnabled()
	return self.isLuaAppStarterScriptEnabled
end

function FlagSettings.IsLuaHomePageEnabled(platform)
	if IsRunningInStudio() then
		return true
	end

	if platform == Enum.Platform.IOS or platform == Enum.Platform.Android then
		return NotificationService.IsLuaHomePageEnabled
	else
		return false
	end
end

function FlagSettings.IsLuaGamesPageEnabled(platform)
	if IsRunningInStudio() then
		return true
	end

	if platform == Enum.Platform.IOS or platform == Enum.Platform.Android then
		return NotificationService.IsLuaGamesPageEnabled
	else
		return false
	end
end

function FlagSettings.IsLuaGamesPagePreloadingEnabled(platform)
	return FlagSettings.IsLuaGamesPageEnabled(platform) and not settings():GetFFlag("LuaAppGamesPagePreloadingDisabled")
end

function FlagSettings.IsLuaBottomBarEnabled()
	return IsRunningInStudio()
end

function FlagSettings.IsLuaAppFriendshipCreatedSignalREnabled()
	return FlagSettings:IsLuaAppStarterScriptEnabled() and settings():GetFFlag("LuaAppFriendshipCreatedSignalREnabled")
end

function FlagSettings.IsLuaAppDeterminingFormFactorAndPlatform()
	return FlagSettings:IsLuaAppStarterScriptEnabled() and settings():GetFFlag("EnableLuaAppFormFactorAndPlatform")
end

function FlagSettings.IsLoadingHUDOniOSEnabledForGameShare()
	return FlagSettings:IsLuaAppStarterScriptEnabled() and settings():GetFFlag("EnableLoadingHUDOniOSForGameShare")
end

function FlagSettings.IsPeopleListV1Enabled()
	return settings():GetFFlag("LuaAppPeopleListV1")
end

function FlagSettings:UseCppTextTruncation()
	return settings():GetFFlag("TextTruncationEnabled")
end

function FlagSettings:UseWebPageWrapperForGameDetails()
	return settings():GetFFlag("LuaUseWebPageWrapperForGameDetails")
end

return FlagSettings