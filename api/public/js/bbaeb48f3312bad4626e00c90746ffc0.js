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

;// bundle: Widgets___AvatarImage___df74b1828a618710b00135ba2b62efef_m
;// files: modules/Widgets/AvatarImage.js

;// modules/Widgets/AvatarImage.js
Roblox.define("Widgets.AvatarImage",[],function(){function r(n){var t=$(n);return{imageSize:t.attr("data-image-size")||"medium",noClick:typeof t.attr("data-no-click")!="undefined",noOverlays:typeof t.attr("data-no-overlays")!="undefined",userId:t.attr("data-user-id")||0,userOutfitId:t.attr("data-useroutfit-id")||0,name:t.attr("data-useroutfit-name")||""}}function u(n,t){if(t.bcOverlayUrl!=null){var i=$("<img>").attr("src",t.bcOverlayUrl).attr("alt","Builders Club").css("position","absolute").css("left","0").css("bottom","0").attr("border",0);n.after(i)}}function n(i,f){for($.type(i)!=="array"&&(i=[i]);i.length>0;){for(var s=i.splice(0,10),o=[],e=0;e<s.length;e++)o.push(r(s[e]));$.getJSON(t.endpoint,{params:JSON.stringify(o)},function(t,i){return function(r){for(var v=[],e,c,h,o=0;o<r.length;o++)if(e=r[o],e!=null){var l=t[o],s=$(l),a=$("<div>").css("position","relative");s.html(a),s=a,i[o].noClick||(c=$("<a>").attr("href",e.url),s.append(c),s=c),h=$("<img>").attr("title",e.name).attr("alt",e.name).attr("border",0),h.load(function(n,t,i,r){return function(){n.width(t.width),n.height(t.height),u(i,r)}}(a,l,h,e)),s.append(h),h.attr("src",e.thumbnailUrl),e.thumbnailFinal||v.push(l)}f=f||1,f<4&&window.setTimeout(function(){n(v,f+1)},f*2e3)}}(s,o))}}function i(){n($(t.selector+":empty").toArray())}var t={selector:".roblox-avatar-image",endpoint:"/avatar-thumbnails?jsoncallback=?"};return{config:t,load:n,populate:i}});


}
/*
     FILE ARCHIVED ON 17:53:26 Jan 30, 2016 AND RETRIEVED FROM THE
     INTERNET ARCHIVE ON 07:33:32 Jul 05, 2024.
     JAVASCRIPT APPENDED BY WAYBACK MACHINE, COPYRIGHT INTERNET ARCHIVE.

     ALL OTHER CONTENT MAY ALSO BE PROTECTED BY COPYRIGHT (17 U.S.C.
     SECTION 108(a)(3)).
*/
/*
playback timings (ms):
  captures_list: 1.258
  exclusion.robots: 0.154
  exclusion.robots.policy: 0.137
  esindex: 0.018
  cdx.remote: 8.768
  LoadShardBlock: 41.679 (3)
  PetaboxLoader3.datanode: 75.397 (5)
  load_resource: 163.522 (2)
  PetaboxLoader3.resolve: 126.215 (2)
*/