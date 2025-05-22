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

    ;// bundle: Widgets___AvatarImage___620fab62f83b3b8873e40b5b3cb1b431_m
    ;// files: modules/Widgets/AvatarImage.js

    ;// modules/Widgets/AvatarImage.js
    Roblox.define("Widgets.AvatarImage",[],function(){function i(n){var t=$(n);return{imageSize:t.attr("data-image-size")||"medium",noClick:typeof t.attr("data-no-click")!="undefined",noOverlays:typeof t.attr("data-no-overlays")!="undefined",userId:t.attr("data-user-id")||0,userOutfitId:t.attr("data-useroutfit-id")||0,name:t.attr("data-useroutfit-name")||""}}function r(n,t){if(t.bcOverlayUrl!=null){var i=$("<img>").attr("src",t.bcOverlayUrl).attr("alt","Builders Club").css("position","absolute").css("left","0").css("bottom","0").attr("border",0).addClass("bc-overlay");n.after(i)}}function t(u,f){for($.type(u)!=="array"&&(u=[u]);u.length>0;){for(var o=u.splice(0,10),s=[],e=0;e<o.length;e++)s.push(i(o[e]));$.getJSON(n.endpoint,{params:JSON.stringify(s)},function(n,i){return function(u){for(var v=[],e,a,h,o=0;o<u.length;o++)if(e=u[o],e!=null){var c=n[o],s=$(c),l=$("<div>").css("position","relative");s.html(l),s=l,i[o].noClick||(a=$("<a>").attr("href",e.url),s.append(a),s=a),h=$("<img>").attr("title",e.name).attr("alt",e.name).attr("border",0),h.load(function(n,t,i,u){return function(){n.width(t.width),n.height(t.height),r(i,u)}}(l,c,h,e)),s.append(h),h.attr("src",e.thumbnailUrl),e.thumbnailFinal||v.push(c)}f=f||1,f<4&&window.setTimeout(function(){t(v,f+1)},f*2e3)}}(o,s))}}function u(){t($(n.selector+":empty").toArray())}var n={selector:".roblox-avatar-image",endpoint:"/avatar-thumbnails?jsoncallback=?"};return{config:n,load:t,populate:u}});


}
/*
     FILE ARCHIVED ON 22:36:06 Feb 26, 2017 AND RETRIEVED FROM THE
     INTERNET ARCHIVE ON 01:54:50 Jun 09, 2023.
     JAVASCRIPT APPENDED BY WAYBACK MACHINE, COPYRIGHT INTERNET ARCHIVE.

     ALL OTHER CONTENT MAY ALSO BE PROTECTED BY COPYRIGHT (17 U.S.C.
     SECTION 108(a)(3)).
*/
/*
playback timings (ms):
  captures_list: 119.439
  exclusion.robots: 0.174
  exclusion.robots.policy: 0.165
  cdx.remote: 0.058
  esindex: 0.008
  LoadShardBlock: 68.542 (3)
  PetaboxLoader3.datanode: 115.725 (5)
  load_resource: 304.078 (2)
  PetaboxLoader3.resolve: 221.589 (2)
*/