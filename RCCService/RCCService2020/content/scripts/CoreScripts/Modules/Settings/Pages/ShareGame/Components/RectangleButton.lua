local CorePackages = game:GetService("CorePackages")

local Roact = require(CorePackages.Roact)

local FFlagLuaInviteModalEnabled = settings():GetFFlag("LuaInviteModalEnabledV384")

local BUTTON_IMAGE = "rbxasset://textures/ui/Settings/MenuBarAssets/MenuButton.png"
local BUTTON_IMAGE_ACTIVE = "rbxasset://textures/ui/Settings/MenuBarAssets/MenuButtonSelected.png"
local BUTTON_SLICE = Rect.new(8, 6, 46, 44)

local DROPSHADOW_SIZE = {
	Left = 4, Right = 4,
	Top = 2, Bottom = 6,
}

local RectangleButton = Roact.PureComponent:extend("RectangleButton")
RectangleButton.defaultProps = {
	visible = true,
}

function RectangleButton:init()
	self.state = {
		isHovering = false,
	}
end

function RectangleButton:render()
	local size = self.props.size
	local position = self.props.position
	local anchorPoint = self.props.anchorPoint
	local layoutOrder = self.props.layoutOrder
	local zIndex = self.props.zIndex
	local onClick = self.props.onClick
	local visible
	if FFlagLuaInviteModalEnabled then
		visible = self.props.visible
	end

	local children = self.props[Roact.Children] or {}

	local buttonImage = self.state.isHovering and BUTTON_IMAGE_ACTIVE or BUTTON_IMAGE

	-- Insert padding so that child elements of this component are positioned
	-- inside the button as expected. This is to offset the dropshadow
	-- extending outside the button bounds.
	children["UIPadding"] = Roact.createElement("UIPadding", {
		PaddingLeft = UDim.new(0, DROPSHADOW_SIZE.Left),
		PaddingRight = UDim.new(0, DROPSHADOW_SIZE.Right),
		PaddingTop = UDim.new(0, DROPSHADOW_SIZE.Top),
		PaddingBottom = UDim.new(0, DROPSHADOW_SIZE.Bottom),
	})

	return Roact.createElement("ImageButton", {
		BackgroundTransparency = 1,
		Image = "",
		Size = size,
		Position = position,
		AnchorPoint = anchorPoint,
		LayoutOrder = layoutOrder,
		ZIndex = zIndex,
		Visible = visible,

		[Roact.Event.InputBegan] = function()
			self:setState({isHovering = true})
		end,
		[Roact.Event.InputEnded] = function()
			self:setState({isHovering = false})
		end,

		[Roact.Event.Activated] = function()
			if onClick then
				self:setState({isHovering = false})
				onClick()
			end
		end,
	}, {
		ButtonBackground = Roact.createElement("ImageLabel", {
			BackgroundTransparency = 1,
			Position = UDim2.new(
				0, -DROPSHADOW_SIZE.Left,
				0, -DROPSHADOW_SIZE.Top
			),
			Size = UDim2.new(
				1, DROPSHADOW_SIZE.Left + DROPSHADOW_SIZE.Right,
				1, DROPSHADOW_SIZE.Top + DROPSHADOW_SIZE.Bottom
			),
			Image = buttonImage,
			ScaleType = Enum.ScaleType.Slice,
			SliceCenter = BUTTON_SLICE,
			ZIndex = zIndex,
		}, children),
	})
end

return RectangleButton
