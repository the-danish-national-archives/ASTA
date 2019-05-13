/*
    Model is responsible for Tabs og realted panels
    inputs array of tag <button> ids contains "-tab-"
    related panels with <div> tag and ids contains "-panel-"
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron')

    var settings = {
        tabId: "-tab-",
        panelId: "-panel-",
        panelIds: []
    }

    Rigsarkiv.Panels = {        
        initialize: function (elemnetIds) {
            var element = null;
            elemnetIds.forEach(elementId => {
                element = document.getElementById(elementId);
                if(element != null) {
                    settings.panelIds.push(elementId.replace(settings.tabId,settings.panelId));
                    element.addEventListener('click', (event) => {
                        var panelId = event.srcElement.id.replace(settings.tabId,settings.panelId);
                        settings.panelIds.forEach(id => {
                            var panel = document.getElementById(id);
                            panel.hidden = (id !== panelId);
                        });                        
                    });
                }
            });
        }
    };
}(jQuery);