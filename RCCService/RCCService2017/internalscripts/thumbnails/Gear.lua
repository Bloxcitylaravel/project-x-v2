-- Gear v1.0.2a

assetUrl, fileExtension, x, y, baseUrl = ...

pcall(function() game:GetService("ContentProvider"):SetBaseUrl(baseUrl) end)
game:GetService("ScriptContext").ScriptsDisabled = true

for _, object in pairs(game:GetObjects(assetUrl)) do
	object.Parent = workspace
end

return game:GetService("ThumbnailGenerator"):Click(fileExtension, x, y, --[[hideSky = ]] true, --[[crop =]] true)