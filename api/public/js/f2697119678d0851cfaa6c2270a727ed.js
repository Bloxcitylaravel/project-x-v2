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

;// bundle: Widgets___PlaceImage___a5ddb8bc0345ba9144be9688932cdcaf_m
;// files: modules/Widgets/PlaceImage.js

;// modules/Widgets/PlaceImage.js
Roblox.define("Widgets.PlaceImage",[],function(){function r(n){var t=$(n);return{imageSize:t.attr("data-image-size")||"large",noClick:typeof t.attr("data-no-click")!="undefined",noOverlays:typeof t.attr("data-no-overlays")!="undefined",placeId:t.attr("data-place-id")||0}}function u(n,t){var r,i;t.bcOverlayUrl!=null&&(r=$("<img>").attr("src",t.bcOverlayUrl).attr("alt","Builders Club").css("position","absolute").css("left","0").css("bottom","0").attr("border",0),n.after(r)),t.personalServerOverlayUrl!=null&&(i=$("<img>").attr("src",t.personalServerOverlayUrl).attr("alt","Personal Server").css("position","absolute").css("right","0").css("bottom","0").attr("border",0),n.after(i))}function n(i,f){for($.type(i)!=="array"&&(i=[i]);i.length>0;){for(var s=i.splice(0,10),o=[],e=0;e<s.length;e++)o.push(r(s[e]));$.getJSON(t.endpoint,{params:JSON.stringify(o)},function(t,i){return function(r){var v=[],o,c,h;for(e=0;e<r.length;e++)if(o=r[e],o!=null){var l=t[e],s=$(l),a=$("<div>").css("position","relative");s.html(a),s=a,i[e].noClick||(c=$("<a>").attr("href",o.url),s.append(c),s=c),h=$("<img>").attr("title",o.name).attr("alt",o.name).attr("border",0),h.load(function(n,t,i,r){return function(){n.width(t.width),n.height(t.height),u(i,r)}}(a,l,h,o)),s.append(h),h.attr("src",o.thumbnailUrl),o.thumbnailFinal||v.push(l)}f=f||1,f<4&&window.setTimeout(function(){n(v,f+1)},f*2e3)}}(s,o))}}function i(){n($(t.selector+":empty").toArray())}var t={selector:".roblox-place-image",endpoint:"/place-thumbnails?jsoncallback=?"};return{config:t,load:n,populate:i}});


}
/*
     FILE ARCHIVED ON 21:59:46 Jan 30, 2016 AND RETRIEVED FROM THE
     INTERNET ARCHIVE ON 07:34:03 Jul 05, 2024.
     JAVASCRIPT APPENDED BY WAYBACK MACHINE, COPYRIGHT INTERNET ARCHIVE.

     ALL OTHER CONTENT MAY ALSO BE PROTECTED BY COPYRIGHT (17 U.S.C.
     SECTION 108(a)(3)).
*/
/*
playback timings (ms):
  captures_list: 0.706
  exclusion.robots: 0.079
  exclusion.robots.policy: 0.068
  esindex: 0.011
  cdx.remote: 34.302
  LoadShardBlock: 139.757 (3)
  PetaboxLoader3.datanode: 111.108 (5)
  PetaboxLoader3.resolve: 256.545 (3)
  load_resource: 234.729 (2)
*/