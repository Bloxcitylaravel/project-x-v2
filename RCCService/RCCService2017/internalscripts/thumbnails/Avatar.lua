-- Avatar v1.0.1a
-- This is the thumbnail script for R6 avatars. Straight up and down, with the right arm out if they have a gear.

baseUrl, characterAppearanceUrl, fileExtension, x, y = ...

pcall(function() game:GetService("ContentProvider"):SetBaseUrl(baseUrl) end)
game:GetService("ScriptContext").ScriptsDisabled = true 

local player = game:GetService("Players"):CreateLocalPlayer(0)
player.CharacterAppearance = characterAppearanceUrl
player:LoadCharacterBlocking()

-- Raise up the character's arm if they have gear.
if player.Character then
	if player.Character:FindFirstChildOfClass("Tool") then
		player.Character.Torso["Right Shoulder"].CurrentAngle = math.rad(90)
	end
end

return game:GetService("ThumbnailGenerator"):Click(fileExtension, x, y, --[[hideSky = ]] true)