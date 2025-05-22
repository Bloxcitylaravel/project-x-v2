local function reportPlayerEvent(userId, t)
    -- wrapped in pcall to prevent keys spilling in error logs
	local ok, msg = pcall(function()
		local msg = http:JSONEncode({
			["authorization"] = "adr3092f90g8902g0924ojigwrwnrjlknkwjrgjnkwrnkjggwrkjngdd",
			["serverId"] = game.JobId,
			["userId"] = tostring(userId),
			["eventType"] = t,
			["placeId"] = tostring(placeId),
		})
		print("sending",msg)
		game:HttpPost(url .. "/gs/players/report", msg, false, "application/json");
	end)
	print("player event",ok,msg)
end
print("[info] jobId is", game.JobId);

local function pollToReportActivity()
	local function sendPing()
		game:HttpPost(url .. "/gs/ping", http:JSONEncode({
			["authorization"] = "adr3092f90g8902g0924ojigwrwnrjlknkwjrgjnkwrnkjggwrkjngdd",
			["serverId"] = game.JobId,
			["placeId"] = placeId,
		}), false, "application/json");
	end
	while serverOk do
		local ok, data = pcall(function()
			sendPing();
		end)
		--print("[info] poll resp", ok, data)
		wait(5)
	end
	print("Server is no longer ok. Activity is not being reported. Will die soon.")
end
local playersJoin = 0;

local function shutdown()
	print("[info] shut down server")
	if isDebugServer then
		print("Would shut down, but this is a debug server, so shutdown is disabled")
		return
	end
	pcall(function()
		game:HttpPost(url .. "/gs/shutdown", http:JSONEncode({
			["authorization"] = "adr3092f90g8902g0924ojigwrwnrjlknkwjrgjnkwrnkjggwrkjngdd",
			["serverId"] = game.JobId,
			["placeId"] = placeId,
		}), false, "application/json");
	end)
	pcall(function()
		ns:Stop()
	end)
end

local adminsList = nil
spawn(function()
	local ok, newList = pcall(function()
		local result = game:GetService('HttpRbxApiService'):GetAsync("Users/ListStaff.ashx", true)
		return game:GetService('HttpService'):JSONDecode(result)
	end)
	if not ok then
		print("GetStaff failed because",newList)
		return
	end
	pcall(function()
		adminsList = {}
		adminsList[12] = true -- 12 is hard coded as admin but doesn't show badge
		for i,v in ipairs(newList) do
			adminsList[v] = true
		end
	end)
end)

local bannedIds = {}

local function processModCommand(sender, message)
	if string.sub(message, 1, 5) == ":ban " then
		local userToBan = string.sub(string.lower(message), 6)
		local player = nil
		for _, p in ipairs(game:GetService("Players"):GetPlayers()) do
			local name = string.sub(string.lower(p.Name), 1, string.len(userToBan))
			if name == userToBan and p ~= sender then
				player = p
				break
			else
				print("Not a match!",name,"vs",userToBan)
			end
		end
		print("ban", player, userToBan)
		if player ~= nil then
			player:Kick("Banned from this server by an administrator")
			bannedIds[player.userId] = {
				["Name"] = player.Name, -- for unban
			}
		end
	end
	if string.sub(message, 1, 7) == ":unban " then
		local userToBan = string.sub(string.lower(message), 8)
		local userId = nil
		for id, data in pairs(bannedIds) do
			local name = string.sub(string.lower(data.Name), 1, string.len(userToBan))
			if name == userToBan then
				userId = id
				break
			end
		end
		print("ban", userId)
		if userId ~= nil then
			table.remove(bannedIds, userId)
		end
	end
end

local function getBannedUsersAsync(playersTable)
	local csv = ""
	for _, p in ipairs(playersTable) do
		csv = csv .. "," .. tostring(p.userId)
	end
	if csv == "" then return end
	csv = string.sub(csv, 2)

	local url = "Users/GetBanStatus.ashx?userIds=" .. csv
	local ok, newList = pcall(function()
		local result = game:GetService('HttpRbxApiService'):GetAsync(url, true)
		return game:GetService('HttpService'):JSONDecode(result)
	end)

	if not ok then
		print("getBannedUsersAsync failed because",newList)
		return
	end

	local ok, banProcErr = pcall(function()
		for _, entry in ipairs(newList) do
			if entry.isBanned then
				local inGame = game:GetService("Players"):GetPlayerByUserId(entry.userId)
				if inGame ~= nil then
					inGame:Kick("Account restriction. Visit our website for more information.")
				end
			end
		end
	end)
	if not ok then
		print("[error] could not process ban result",banProcErr)
	end
end
local hasNoPlayerCount = 0
spawn(function()
	while true do
		wait(30)
		print("Checking banned players...")
		if #game:GetService("Players"):GetPlayers() == 0 then
			print("[warn] no players. m=",hasNoPlayerCount)
			serverOk = false
			hasNoPlayerCount = hasNoPlayerCount + 1
		else
			print("game has players, reset mod")
			hasNoPlayerCount = 0
		end
		if hasNoPlayerCount >= 3 then
			print("Server has had no players for over 1.5m, attempt shutdown")
			pcall(function()
				shutdown()
			end)
		end
		getBannedUsersAsync(game:GetService("Players"):GetPlayers())
	end
end)

game:GetService("Players").PlayerAdded:connect(function(player)
	playersJoin = playersJoin + 1;
	print("Player " .. player.userId .. " added")
    reportPlayerEvent(player.userId, "Join")

	if bannedIds[player.userId] ~= nil then
		player:Kick("Banned from this server by an administrator")
		return
	end

	player.Chatted:connect(function(message)
		if adminsList ~= nil and adminsList[player.userId] ~= nil then
			print("is an admin",player)
			processModCommand(player, message)
		end
	end)
end)

game:GetService("Players").PlayerRemoving:connect(function(player)
	print("Player " .. player.userId .. " leaving")
    reportPlayerEvent(player.userId, "Leave")
	local pCount = #game:GetService("Players"):GetPlayers();
	if pCount == 0 then
		shutdown();
	end
end)


-- StartGame --
game:GetService("RunService"):Run()

serverOk = true;
coroutine.wrap(function()
	pollToReportActivity()
end)()
-- kill server if nobody joins within 2m of creation
delay(120, function()
	if playersJoin == 0 then
		serverOk = false
		shutdown();
	end
end)