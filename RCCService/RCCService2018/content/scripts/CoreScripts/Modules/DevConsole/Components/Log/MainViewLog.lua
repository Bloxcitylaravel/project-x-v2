local CorePackages = game:GetService("CorePackages")
local Roact = require(CorePackages.Roact)
local RoactRodux = require(CorePackages.RoactRodux)

local Components = script.Parent.Parent.Parent.Components
local ClientLog = require(Components.Log.ClientLog)
local ServerLog = require(Components.Log.ServerLog)
local UtilAndTab = require(Components.UtilAndTab)
local DataConsumer = require(Components.DataConsumer)

local Actions = script.Parent.Parent.Parent.Actions
local ClientLogUpdateSearchFilter = require(Actions.ClientLogUpdateSearchFilter)
local ServerLogUpdateSearchFilter = require(Actions.ServerLogUpdateSearchFilter)

local Constants = require(script.Parent.Parent.Parent.Constants)
local PADDING = Constants.GeneralFormatting.MainRowPadding

local MsgTypeNamesOrdered = Constants.MsgTypeNamesOrdered

local MainViewLog = Roact.PureComponent:extend("MainViewLog")

function MainViewLog:init()
	self.onUtilTabHeightChanged = function(utilTabHeight)
		self:setState({
			utilTabHeight = utilTabHeight
		})
	end

	self.onClientButton = function()
		self:setState({isClientView = true})
	end

	self.onServerButton = function()
		self:setState({isClientView = false})
	end

	self.onCheckBoxChanged = function(boxName, newState)
		if self.state.isClientView then
			self.props.ClientLogData:setFilter(boxName, newState)
		else
			self.props.ServerLogData:setFilter(boxName, newState)
		end
	end

	self.filterUpdated = function()
		self:setState({})
	end

	self.onSearchTermChanged = function(newSearchTerm)
		if self.state.isClientView then
			self.props.ClientLogData:setSearchTerm(newSearchTerm)
		else
			self.props.ServerLogData:setSearchTerm(newSearchTerm)
		end
	end

	local clientFilterSignal = self.props.ClientLogData:filterUpdatedSignal()
	self.clientFilterConnection = clientFilterSignal:Connect(self.filterUpdated)

	local serverFilterSignal = self.props.ServerLogData:filterUpdatedSignal()
	self.serverFilterConnection = serverFilterSignal:Connect(self.filterUpdated)

	self.utilRef = Roact.createRef()

	self.state = {
		utilTabHeight = 0,
		isClientView = true
	}
end

function MainViewLog:didMount()
	local utilSize = self.utilRef.current.Size
	self:setState({
		utilTabHeight = utilSize.Y.Offset
	})
end

function MainViewLog:didUpdate()
	local utilSize = self.utilRef.current.Size
	if utilSize.Y.Offset ~= self.state.utilTabHeight then
		self:setState({
			utilTabHeight = utilSize.Y.Offset
		})
	end
end

function MainViewLog:willUnmount()
	if self.clientFilterConnection then
		self.clientFilterConnection:Disconnect()
		self.clientFilterConnection = nil
	end
	if self.serverFilterConnection then
		self.serverFilterConnection:Disconnect()
		self.serverFilterConnection = nil
	end
end

function MainViewLog:render()
	local size = self.props.size
	local formFactor = self.props.formFactor
	local isdeveloperView = self.props.isdeveloperView
	local tabList = self.props.tabList

	local utilTabHeight = self.state.utilTabHeight
	local isClientView = self.state.isClientView

	local searchTerm
	if isClientView then
		searchTerm = self.props.ClientLogData:getSearchTerm()
	else
		searchTerm = self.props.ServerLogData:getSearchTerm()
	end

	local elements = {}

	elements["UIListLayout"] = Roact.createElement("UIListLayout", {
		SortOrder = Enum.SortOrder.LayoutOrder,
		Padding = UDim.new(0, PADDING),
	})

	local currCheckBoxState
	if isClientView then
		currCheckBoxState = self.props.ClientLogData:getFilters()
	else
		currCheckBoxState = self.props.ServerLogData:getFilters()
	end

	local initCheckBoxes = {}
	for i, v in  ipairs(MsgTypeNamesOrdered) do
		initCheckBoxes[i] = {
			name = v,
			state = currCheckBoxState[v],
		}
	end

	elements ["UtilAndTab"] = Roact.createElement(UtilAndTab, {
		windowWidth = size.X.Offset,
		formFactor = formFactor,
		tabList = tabList,
		orderedCheckBoxState = initCheckBoxes,
		isClientView = isClientView,
		searchTerm = searchTerm,
		layoutOrder = 1,

		refForParent = self.utilRef,

		onClientButton = isdeveloperView and self.onClientButton,
		onServerButton = isdeveloperView and self.onServerButton,
		onCheckBoxChanged = self.onCheckBoxChanged,
		onSearchTermChanged = self.onSearchTermChanged,
	})

	if utilTabHeight > 0 then
		if isClientView then
			elements["ClientLog"] = Roact.createElement(ClientLog, {
				size = UDim2.new(1, 0, 1, -utilTabHeight),
				layoutOrder = 2,
			})
		else
			elements["ServerLog"] = Roact.createElement(ServerLog, {
				size = UDim2.new(1, 0, 1, -utilTabHeight),
				layoutOrder = 2,
			})
		end
	end

	return Roact.createElement("Frame", {
		Size = size,
		BackgroundColor3 = Constants.Color.BaseGray,
		BackgroundTransparency = 1,
		LayoutOrder = 3,

	}, elements)
end

local function mapDispatchToProps(dispatch)
	return {
		dispatchClientLogUpdateSearchFilter = function(searchTerm, filters)
			dispatch(ClientLogUpdateSearchFilter(searchTerm, filters))
		end,

		dispatchServerLogUpdateSearchFilter = function(searchTerm, filters)
			dispatch(ServerLogUpdateSearchFilter(searchTerm, filters))
		end,
	}
end

return RoactRodux.UNSTABLE_connect2(nil, mapDispatchToProps)(
	DataConsumer(MainViewLog, "ClientLogData", "ServerLogData")
)
