-- Clothing v1.0.0
-- This used to be Pants.lua and Shirt.lua but since splitting them into two different scripts is pointless when they do the same thing, I just migrated them into one script.

assetUrl, fileExtension, x, y, baseUrl, mannequinId = ...

pcall(function() game:GetService("ContentProvider"):SetBaseUrl(baseUrl) end)
game:GetService("ScriptContext").ScriptsDisabled = true

local mannequin = game:GetObjects(baseUrl.. "asset/?id=" .. tostring(mannequinId))[1]
mannequin.Humanoid.DisplayDistanceType = Enum.HumanoidDisplayDistanceType.None
mannequin.Parent = workspace

local clothing = game:GetObjects(assetUrl)[1]
clothing.Parent = mannequin

return game:GetService("ThumbnailGenerator"):Click(fileExtension, x, y, --[[hideSky = ]] true)