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

    ;// bundle: Widgets___SurveyModal___b849f2e5a82baf30e4a3eb0f058db679_m
    ;// files: modules/Widgets/SurveyModal.js

    ;// modules/Widgets/SurveyModal.js
    typeof Roblox=="undefined"&&(Roblox={}),typeof Roblox.SurveyModal=="undefined"&&(Roblox.SurveyModal=function(){function t(){$('[data-modal-handle="survey"]').find("iframe").show(),$('[data-modal-handle="survey"]').modal(i)}function n(){$.modal.close(),$('[data-modal-handle="survey"]').find("iframe").hide()}var i={overlayClose:!0,escClose:!0,opacity:80,overlayCss:{backgroundColor:"#000"},onClose:n};return{open:t}}());


}