var _____WB$wombat$assign$function_____ = function(name) {return (self._wb_wombat && self._wb_wombat.local_init && self._wb_wombat.local_init(name)) || self[name]; };
if (!self.__WB_pmw) { self.__WB_pmw = function(obj) { this.__WB_source = obj; return this; } }
{
    let window = _____WB$wombat$assign$function_____("window");
    let self = _____WB$wombat$assign$function_____("self");
    let document = _____WB$wombat$assign$function_____("document");
    let location = _____WB$wombat$assign$function_____("location");
    let top = _____WB$wombat$assign$function_____("top");
    let parent = _____WB$wombat$assign$function_____("parent");
    let frames = _____WB$wombat$assign$function_____("frames");
    let opener = _____WB$wombat$assign$function_____("opener");

    ;// bundle: Widgets___DropdownMenu___1b78138201eaf5ac3f85361ff82eddc2_m
    ;// files: modules/Widgets/DropdownMenu.js

    ;// modules/Widgets/DropdownMenu.js
    Roblox.define("Widgets.DropdownMenu",[],function(){function t(n){$(n).on("click",".button",function(){var n=$(this),i,t;return n.hasClass("init")||(i=$(this).outerWidth()-parseInt(n.css("border-left-width"))-parseInt(n.css("border-right-width")),n.siblings(".dropdown-list").css("min-width",i),t=n.siblings('.dropdown-list[data-align="right"]').first(),t.css("right",0),n.addClass("init")),n.hasClass("active")?(n.removeClass("active"),n.siblings(".dropdown-list").hide()):(n.addClass("active"),n.siblings(".dropdown-list").show()),$(document).click(function(){$(".button.init.active").removeClass("active"),$(".dropdown-list").hide()}),!1})}function n(){var n=$(".button").not(".init");n.each(function(){var t=$(this).outerWidth()-parseInt($(this).css("border-left-width"))-parseInt($(this).css("border-right-width")),n;$(this).siblings(".dropdown-list").css("min-width",t),n=$(this).siblings('.dropdown-list[data-align="right"]').first(),n.css("right",0)}),$(".dropdown-list").hide(),n.click(function(){return $(this).hasClass("active")?($(this).removeClass("active"),$(this).siblings(".dropdown-list").hide()):($(this).addClass("active"),$(this).siblings(".dropdown-list").show()),!1}),$(document).click(function(){n.removeClass("active"),$(".dropdown-list").hide()}),n.addClass("init")}return{InitializeDropdown:n,LazyInitializeDropdown:t}});


}