﻿var _____WB$wombat$assign$function_____ = function(name) {return (self._wb_wombat && self._wb_wombat.local_init && self._wb_wombat.local_init(name)) || self[name]; };
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

    ;// bundle: Pages___CatalogShared___97fa73721c4c1e1f176ba3b56f3350ba_m
    ;// files: modules/Pages/CatalogShared.js

    ;// modules/Pages/CatalogShared.js
    typeof Roblox==typeof undefined&&(Roblox={}),Roblox.CatalogShared=Roblox.CatalogShared||{},Roblox.CatalogSharedConstructor=function(n){function u(n,t,u,f,e){if(n&&u&&u.length!==0){i+=1;var o=i;Roblox.AjaxPageLoadEvent&&Roblox.AjaxPageLoadEvent.SendEvent("legacyCatalog",n),u.find(".right-content").append($('<div class="loading"></div>')),u.find(".subcategories [hover='true']").css("display","none"),u.css("cursor","progress"),$.ajax({method:"GET",params:t,url:n,crossDomain:!0,xhrFields:{withCredentials:!0}}).done(function(t){if(i==o&&(u.html(t),u.css("cursor","default"),!f)){var s=$.Event(r,{url:n,replaceCurrentState:e});u.trigger(s)}}).fail(function(){var t=$(".right-content"),i;t.find(".error-message").length==0&&(i=$("<div>Catalog temporarily unavailable, please try again later.</div>").addClass("error-message"),t.prepend(i)),t.find(".loading").remove()})}}function f(i){var r,u;!t&&i.clickTargetID&&(doNotUpdateHistory=!0,i.clickTargetID==="catalog"?(r=i.url?i.url.split("?")[1]:n.URL.split("?")[1],r&&Roblox.CatalogValues&&Roblox.CatalogValues.CatalogContentsUrl?(u=$("#"+Roblox.CatalogValues.ContainerID),Roblox.CatalogShared.LoadCatalogAjax(Roblox.CatalogValues.CatalogContentsUrl+"?"+r,null,u,!0)):window.location.href=i.url):$("#"+i.clickTargetID).click(),doNotUpdateHistory=!1)}function e(i){var u,r,o,s,f,e;Roblox.AdsHelper&&Roblox.AdsHelper.AdRefresher&&Roblox.AdsHelper.AdRefresher.refreshAds(),i.url&&(u=i.url.split("?")[1],u&&(r=n.URL.split("?")[0].toLowerCase(),r=r.indexOf("#")===-1?r:r.split("#")[0],r=r.replace("catalog/default.aspx","catalog/"),r.indexOf("browse.aspx")<0&&r.indexOf("/develop/library")<0&&(o=r.length,s=r.lastIndexOf("/")===o-1,r+=s?"browse.aspx":"/browse.aspx"),f=r+"?"+u,$("#LibraryTabLink").attr("data-query-params",u),e=r.indexOf("/develop/library")>=0?"/develop/library/?"+u:"/catalog/?"+u,GoogleAnalyticsEvents&&GoogleAnalyticsEvents.ViewVirtual(e),t=!0,i.replaceCurrentState?History.replaceState({clickTargetID:"catalog",url:f},n.title,f):History.pushState({clickTargetID:"catalog",url:f},n.title,f),t=!1))}var r="CatalogLoadedViaAjax",t=!1,i=0;return{LoadCatalogAjax:u,CatalogLoadedViaAjaxEventName:r,handleURLChange:f,handleCatalogLoadedViaAjaxEvent:e}},Roblox.CatalogShared=Roblox.CatalogSharedConstructor(document);


}