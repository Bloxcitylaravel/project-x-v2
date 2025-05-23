--	// FileName: Util.lua
--	// Written by: Xsitsu, TheGamer101
--	// Description: Module for shared code between MessageCreatorModules.

--[[
Creating a message creator module:
1) Create a new module inside the MessageCreatorModules folder.
2) Create a function that takes a messageData object and returns:
{
	KEY_BASE_FRAME = BaseFrame,
	KEY_UPDATE_TEXT_FUNC = function(newMessageObject) ---Function to update the text of the message.
	KEY_GET_HEIGHT = function() ---Function to get the height of the message in absolute pixels,
	KEY_FADE_IN = function(duration, CurveUtil) ---Function to tell the message to start fading in.
	KEY_FADE_OUT = function(duration, CurveUtil) ---Function to tell the message to start fading out.
	KEY_UPDATE_ANIMATION = function(dtScale, CurveUtil) ---Update animation function.
}
3) return the following format from the module:
{
	KEY_MESSAGE_TYPE = "Message type this module creates messages for."
	KEY_CREATOR_FUNCTION = YourFunctionHere
}
--]]

local DEFAULT_MESSAGE_CREATOR = "UnknownMessage"
local MESSAGE_CREATOR_MODULES_VERSION = 1
---Creator Module Object Keys
local KEY_MESSAGE_TYPE = "MessageType"
local KEY_CREATOR_FUNCTION = "MessageCreatorFunc"
---Creator function return object keys
local KEY_BASE_FRAME = "BaseFrame"
local KEY_UPDATE_TEXT_FUNC = "UpdateTextFunction"
local KEY_GET_HEIGHT = "GetHeightFunction"
local KEY_FADE_IN = "FadeInFunction"
local KEY_FADE_OUT = "FadeOutFunction"
local KEY_UPDATE_ANIMATION = "UpdateAnimFunction"

local module = {}
local methods = {}
methods.__index = methods

local testLabel = Instance.new("TextLabel")
testLabel.Selectable = false
testLabel.TextWrapped = true
testLabel.Position = UDim2.new(1, 0, 1, 0)

function WaitUntilParentedCorrectly()
	while (not testLabel:IsDescendantOf(game:GetService("Players").LocalPlayer)) do
		testLabel.AncestryChanged:wait()
	end
end

local TextSizeCache = {}
function methods:GetStringTextBounds(text, font, textSize, sizeBounds)
	WaitUntilParentedCorrectly()
	sizeBounds = sizeBounds or false
	if not TextSizeCache[text] then
		TextSizeCache[text] = {}
	end
	if not TextSizeCache[text][font] then
		TextSizeCache[text][font] = {}
	end
	if not TextSizeCache[text][font][sizeBounds] then
		TextSizeCache[text][font][sizeBounds] = {}
	end
	if not TextSizeCache[text][font][sizeBounds][textSize] then
		testLabel.Text = text
		testLabel.Font = font
		testLabel.TextSize = textSize
		if sizeBounds then
			testLabel.TextWrapped = true;
			testLabel.Size = sizeBounds
		else
			testLabel.TextWrapped = false;
		end
		TextSizeCache[text][font][sizeBounds][textSize] = testLabel.TextBounds
	end
	return TextSizeCache[text][font][sizeBounds][textSize]
end
--// Above was taken directly from Util.GetStringTextBounds() in the old chat corescripts.

function methods:GetMessageHeight(BaseMessage, BaseFrame)
	local textBoundsSize = self:GetStringTextBounds(BaseMessage.Text, BaseMessage.Font, BaseMessage.TextSize, UDim2.new(0, BaseFrame.AbsoluteSize.X, 0, 1000))
	return textBoundsSize.Y
end

function methods:GetNumberOfSpaces(str, font, textSize)
	local strSize = self:GetStringTextBounds(str, font, textSize)
	local singleSpaceSize = self:GetStringTextBounds(" ", font, textSize)
	return math.ceil(strSize.X / singleSpaceSize.X)
end

function methods:CreateBaseMessage(message, font, textSize, chatColor)
	local BaseFrame = self:GetFromObjectPool("Frame")
	BaseFrame.Selectable = false
	BaseFrame.Size = UDim2.new(1, 0, 0, 18)
	BaseFrame.BackgroundTransparency = 1

	local messageBorder = 8

	local BaseMessage = self:GetFromObjectPool("TextLabel")
	BaseMessage.Selectable = false
	BaseMessage.Size = UDim2.new(1, -(messageBorder + 6), 1, 0)
	BaseMessage.Position = UDim2.new(0, messageBorder, 0, 0)
	BaseMessage.BackgroundTransparency = 1
	BaseMessage.Font = font
	BaseMessage.TextSize = textSize
	BaseMessage.TextXAlignment = Enum.TextXAlignment.Left
	BaseMessage.TextYAlignment = Enum.TextYAlignment.Top
	BaseMessage.TextTransparency = 0
	BaseMessage.TextStrokeTransparency = 0.75
	BaseMessage.TextColor3 = chatColor
	BaseMessage.TextWrapped = true
	BaseMessage.Text = message
	BaseMessage.Parent = BaseFrame

	return BaseFrame, BaseMessage
end

function methods:AddNameButtonToBaseMessage(BaseMessage, nameColor, formatName)
	local speakerNameSize = self:GetStringTextBounds(formatName, BaseMessage.Font, BaseMessage.TextSize)
	local NameButton = self:GetFromObjectPool("TextButton")
	NameButton.Selectable = false
	NameButton.Size = UDim2.new(0, speakerNameSize.X, 0, speakerNameSize.Y)
	NameButton.Position = UDim2.new(0, 0, 0, 0)
	NameButton.BackgroundTransparency = 1
	NameButton.Font = BaseMessage.Font
	NameButton.TextSize = BaseMessage.TextSize
	NameButton.TextXAlignment = BaseMessage.TextXAlignment
	NameButton.TextYAlignment = BaseMessage.TextYAlignment
	NameButton.TextTransparency = BaseMessage.TextTransparency
	NameButton.TextStrokeTransparency = BaseMessage.TextStrokeTransparency
	NameButton.TextColor3 = nameColor
	NameButton.Text = formatName
	NameButton.Parent = BaseMessage
	return NameButton
end

function methods:AddChannelButtonToBaseMessage(BaseMessage, formatChannelName)
	local channelNameSize = self:GetStringTextBounds(formatChannelName, BaseMessage.Font, BaseMessage.TextSize)
	local ChannelButton = self:GetFromObjectPool("TextButton")
	ChannelButton.Selectable = false
	ChannelButton.Size = UDim2.new(0, channelNameSize.X, 0, channelNameSize.Y)
	ChannelButton.Position = UDim2.new(0, 0, 0, 0)
	ChannelButton.BackgroundTransparency = 1
	ChannelButton.Font = BaseMessage.Font
	ChannelButton.TextSize = BaseMessage.TextSize
	ChannelButton.TextXAlignment = BaseMessage.TextXAlignment
	ChannelButton.TextYAlignment = BaseMessage.TextYAlignment
	ChannelButton.TextTransparency = BaseMessage.TextTransparency
	ChannelButton.TextStrokeTransparency = BaseMessage.TextStrokeTransparency
	ChannelButton.TextColor3 = BaseMessage.TextColor3
	ChannelButton.Text = formatChannelName
	ChannelButton.Parent = BaseMessage
	return ChannelButton
end

function methods:GetFromObjectPool(className)
	if self.ObjectPool == nil then
		return Instance.new(className)
	end
	return self.ObjectPool:GetInstance(className)
end

function methods:RegisterObjectPool(objectPool)
	self.ObjectPool = objectPool
end

function methods:RegisterGuiRoot(root)
	testLabel.Parent = root
end

function module.new()
	local obj = setmetatable({}, methods)

	obj.ObjectPool = nil
	obj.DEFAULT_MESSAGE_CREATOR = DEFAULT_MESSAGE_CREATOR
	obj.MESSAGE_CREATOR_MODULES_VERSION = MESSAGE_CREATOR_MODULES_VERSION

	obj.KEY_MESSAGE_TYPE = KEY_MESSAGE_TYPE
	obj.KEY_CREATOR_FUNCTION = KEY_CREATOR_FUNCTION

	obj.KEY_BASE_FRAME = KEY_BASE_FRAME
	obj.KEY_UPDATE_TEXT_FUNC = KEY_UPDATE_TEXT_FUNC
	obj.KEY_GET_HEIGHT = KEY_GET_HEIGHT
	obj.KEY_FADE_IN = KEY_FADE_IN
	obj.KEY_FADE_OUT = KEY_FADE_OUT
	obj.KEY_UPDATE_ANIMATION = KEY_UPDATE_ANIMATION

	return obj
end

return module.new()
