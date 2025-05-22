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

    ;// bundle: Widgets___ItemImage___1b3eed95691f4dd4a22c85c2921c95ee_m
    ;// files: modules/Widgets/ItemImage.js

    ;// modules/Widgets/ItemImage.js
    Roblox.define("Widgets.ItemImage",[],function(){function i(n){var t=$(n);return{imageSize:t.attr("data-image-size")||"large",noClick:typeof t.attr("data-no-click")!="undefined",noOverlays:typeof t.attr("data-no-overlays")!="undefined",assetId:t.attr("data-item-id")||0}}function t(r,u){for($.type(r)!=="array"&&(r=[r]);r.length>0;){for(var e=r.splice(0,10),o=[],f=0;f<e.length;f++)o.push(i(e[f]));$.getJSON(n.endpoint,{params:JSON.stringify(o)},function(n,i){return function(r){for(var a=[],f,l,s,e=0;e<r.length;e++)if(f=r[e],f!=null){var h=n[e],o=$(h),c=$("<div>").css("position","relative").css("overflow","hidden");o.html(c),o=c,i[e].noClick||(l=$("<a>").attr("href",f.url),o.append(l),o=l),s=$("<img>").attr("title",f.name).attr("alt",f.name).attr("border",0).addClass("original-image modal-thumb"),s.load(function(n,t){return function(){n.width(t.width),n.height(t.height)}}(c,h,s,f)),o.append(s),s.attr("src",f.thumbnailUrl),f.thumbnailFinal||a.push(h)}u=u||1,u<4&&window.setTimeout(function(){t(a,u+1)},u*2e3)}}(e,o))}}function r(){t($(n.selector+":empty").toArray())}var n={selector:".roblox-item-image",endpoint:"/item-thumbnails?jsoncallback=?"};return{config:n,load:t,populate:r}});


}