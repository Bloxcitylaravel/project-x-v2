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

    ;// bundle: Widgets___PlaceImage___1ae9e66f4e65dbaecfe81a4f2f743358_m
    ;// files: modules/Widgets/PlaceImage.js

    ;// modules/Widgets/PlaceImage.js
    Roblox.define("Widgets.PlaceImage",[],function(){function i(n){var t=$(n);return{imageSize:t.attr("data-image-size")||"large",noClick:typeof t.attr("data-no-click")!="undefined",noOverlays:typeof t.attr("data-no-overlays")!="undefined",placeId:t.attr("data-place-id")||0}}function r(n,t){if(t.bcOverlayUrl!=null){var i=$("<img>").attr("src",t.bcOverlayUrl).attr("alt","Builders Club").css("position","absolute").css("left","0").css("bottom","0").attr("border",0);n.after(i)}}function t(u,f){for($.type(u)!=="array"&&(u=[u]);u.length>0;){for(var o=u.splice(0,10),s=[],e=0;e<o.length;e++)s.push(i(o[e]));$.getJSON(n.endpoint,{params:JSON.stringify(s)},function(n,i){return function(u){var v=[],o,a,h;for(e=0;e<u.length;e++)if(o=u[e],o!=null){var c=n[e],s=$(c),l=$("<div>").css("position","relative");s.html(l),s=l,i[e].noClick||(a=$("<a>").attr("href",o.url),s.append(a),s=a),h=$("<img>").attr("title",o.name).attr("alt",o.name).attr("border",0),h.load(function(n,t,i,u){return function(){n.width(t.width),n.height(t.height),r(i,u)}}(l,c,h,o)),s.append(h),h.attr("src",o.thumbnailUrl),o.thumbnailFinal||v.push(c)}f=f||1,f<4&&window.setTimeout(function(){t(v,f+1)},f*2e3)}}(o,s))}}function u(){t($(n.selector+":empty").toArray())}var n={selector:".roblox-place-image",endpoint:"/place-thumbnails?jsoncallback=?"};return{config:n,load:t,populate:u}});


}