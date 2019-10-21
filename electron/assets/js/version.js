/*
    Model is responsible for version (admin, none admin)
    initialize interface inputs: 
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const electron = require('electron');

    //private data members
    var settings = {
        outputErrorSpn: null,
        outputErrorText: null,
        versionNo: null
    }

    //Add links events
    var UpdateLinks = function(elemnetIds) {
        var element = null;
        var languageCallback = Rigsarkiv.Language.callback();
        elemnetIds.forEach(elementId => {
            element = document.getElementById(elementId);
            if(element != null) {
               element.innerHTML = languageCallback.getValue("welcome-versionfooter").format(settings.versionNo);
            }
            else {
                console.logInfo(`none exist elment with id: ${elementId}`,"Rigsarkiv.Version.initialize");
            }  
        }); 
    }

    //Model interfaces functions
    Rigsarkiv.Version = {
        initialize: function (outputErrorId) {
            settings.outputErrorSpn = document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML;
            try
            {
                settings.versionNo = electron.remote["app"].getVersion();
            }
            catch(err) 
            {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Version.initialize"); 
            } 
        },
        callback: function () {
            return { 
                updateLinks: function(elemnetIds) {
                    UpdateLinks(elemnetIds);   
                }
            }
        }
    }
}(jQuery);