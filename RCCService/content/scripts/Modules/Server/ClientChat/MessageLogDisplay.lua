--	// FileName: MessageLogDisplay.lua
--	// Written by: Xsitsu, TheGamer101
--	// Description: ChatChannel window for displaying messages.

local module = {}
module.ScrollBarThickness = 4

--////////////////////////////// Include
--//////////////////////////////////////
local Chat = game:GetService("Chat")
local clientChatModules = Chat:WaitForChild("ClientChatModules")
local modulesFolder = script.Parent
local moduleMessageLabelCreator = require(modulesFolder:WaitForChild("MessageLabelCreator"))
local ClassMaker = require(modulesFolder:WaitForChild("ClassMaker"))
local CurveUtil = require(modulesFolder:WaitForChild("CurveUtil"))

local ChatSettings = require(clientChatModules:WaitForChild("ChatSettings"))

local MessageLabelCreator = moduleMessageLabelCreator.new()

--////////////////////////////// Methods
--//////////////////////////////////////
local methods = {}

local function CreateGuiObjects()
	local BaseFrame = Instance.new("Frame")
	BaseFrame.Selectable = false
	BaseFrame.Size = UDim2.new(1, 0, 1, 0)
	BaseFrame.BackgroundTransparency = 1

	local Scroller = Instance.new("ScrollingFrame")
	Scroller.Selectable = ChatSettings.GamepadNavigationEnabled
	Scroller.Name = "Scroller"
	Scroller.BackgroundTransparency = 1
	Scroller.BorderSizePixel = 0
	Scroller.Position = UDim2.new(0, 0, 0, 3)
	Scroller.Size = UDim2.new(1, -4, 1, -6)
	Scroller.CanvasSize = UDim2.new(0, 0, 0, 0)
	Scroller.ScrollBarThickness = module.ScrollBarThickness
	Scroller.Active = false
	Scroller.Parent = BaseFrame

	return BaseFrame, Scroller
end

function methods:Destroy()
	self.GuiObject:Destroy()
	self.Destroyed = true
end

function methods:SetActive(active)
	self.GuiObject.Visible = active
end

function methods:UpdateMessageFiltered(messageData)
	local messageObject = nil
	local searchIndex = 1
	local searchTable = self.MessageObjectLog

	while (#searchTable >= searchIndex) do
		local obj = searchTable[searchIndex]

		if (obj.ID == messageData.ID) then
			messageObject = obj
			break
		end

		searchIndex = searchIndex + 1
	end

	if (messageObject) then
		messageObject.UpdateTextFunction(messageData)
		self:ReorderAllMessages()
	end
end

function methods:AddMessage(messageData)
	self:WaitUntilParentedCorrectly()

	local messageObject = MessageLabelCreator:CreateMessageLabel(messageData, self.CurrentChannelName)
	if messageObject == nil then
		return
	end

	table.insert(self.MessageObjectLog, messageObject)
	self:PositionMessageLabelInWindow(messageObject)
end

function methods:RemoveLastMessage()
	self:WaitUntilParentedCorrectly()

	local lastMessage = self.MessageObjectLog[1]
	local posOffset = UDim2.new(0, 0, 0, lastMessage.BaseFrame.AbsoluteSize.Y)

	lastMessage:Destroy()
	table.remove(self.MessageObjectLog, 1)

	for i, messageObject in pairs(self.MessageObjectLog) do
		messageObject.BaseFrame.Position = messageObject.BaseFrame.Position - posOffset
	end

	self.Scroller.CanvasSize = self.Scroller.CanvasSize - posOffset
end

function methods:PositionMessageLabelInWindow(messageObject)
	self:WaitUntilParentedCorrectly()

	local baseFrame = messageObject.BaseFrame

	baseFrame.Parent = self.Scroller
	baseFrame.Position = UDim2.new(0, 0, 0, self.Scroller.CanvasSize.Y.Offset)

	baseFrame.Size = UDim2.new(1, 0, 0, messageObject.GetHeightFunction())

	local scrollBarBottomPosition = (self.Scroller.CanvasSize.Y.Offset - self.Scroller.AbsoluteSize.Y)
	local reposition = (self.Scroller.CanvasPosition.Y >= scrollBarBottomPosition)

	local add = UDim2.new(0, 0, 0, baseFrame.Size.Y.Offset)
	self.Scroller.CanvasSize = self.Scroller.CanvasSize + add

	if (reposition) then
		self.Scroller.CanvasPosition = Vector2.new(0, math.max(0, self.Scroller.CanvasSize.Y.Offset - self.Scroller.AbsoluteSize.Y))
	end
end

function methods:ReorderAllMessages()
	self:WaitUntilParentedCorrectly()

	--// Reordering / reparenting with a size less than 1 causes weird glitches to happen with scrolling as repositioning happens.
	if (self.GuiObject.AbsoluteSize.Y < 1) then return end

	self.Scroller.CanvasSize = UDim2.new(0, 0, 0, 0)
	for i, messageObject in pairs(self.MessageObjectLog) do
		self:PositionMessageLabelInWindow(messageObject)
	end
end

function methods:Clear()
	for i, v in pairs(self.MessageObjectLog) do
		v:Destroy()
	end
	rawset(self, "MessageObjectLog", {})

	self.Scroller.CanvasSize = UDim2.new(0, 0, 0, 0)
end

function methods:SetCurrentChannelName(name)
	self.CurrentChannelName = name
end

function methods:FadeOutBackground(duration)
	--// Do nothing
end

function methods:FadeInBackground(duration)
	--// Do nothing
end

function methods:FadeOutText(duration)
	for i = 1, #self.MessageObjectLog do
		if self.MessageObjectLog[i].FadeOutFunction then
			self.MessageObjectLog[i].FadeOutFunction(duration, CurveUtil)
		end
	end
end

function methods:FadeInText(duration)
	for i = 1, #self.MessageObjectLog do
		if self.MessageObjectLog[i].FadeInFunction then
			self.MessageObjectLog[i].FadeInFunction(duration, CurveUtil)
		end
	end
end

function methods:Update(dtScale)
	for i = 1, #self.MessageObjectLog do
		if self.MessageObjectLog[i].UpdateAnimFunction then
			self.MessageObjectLog[i].UpdateAnimFunction(dtScale, CurveUtil)
		end
	end
end

--// ToDo: Move to common modules
function methods:WaitUntilParentedCorrectly()
	while (not self.GuiObject:IsDescendantOf(game:GetService("Players").LocalPlayer)) do
		self.GuiObject.AncestryChanged:wait()
	end
end

--///////////////////////// Constructors
--//////////////////////////////////////
ClassMaker.RegisterClassType("MessageLogDisplay", methods)

function module.new()
	local obj = {}
	obj.Destroyed = false

	local BaseFrame, Scroller = CreateGuiObjects()
	obj.GuiObject = BaseFrame
	obj.Scroller = Scroller

	obj.MessageObjectLog = {}

	obj.Name = "MessageLogDisplay"
	obj.GuiObject.Name = "Frame_" .. obj.Name

	obj.CurrentChannelName = ""

	ClassMaker.MakeClass("MessageLogDisplay", obj)

	obj.GuiObject.Changed:connect(function(prop)
		if (prop == "AbsoluteSize") then
			spawn(function() obj:ReorderAllMessages() end)
		end
	end)

	return obj
end

return module
