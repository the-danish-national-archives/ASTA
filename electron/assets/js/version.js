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
        footerParagraph: null,
        footerText: null,
        versionNo: null
    }

    //Model interfaces functions
    Rigsarkiv.Version = {
        initialize: function (outputErrorId,footerId) {
            settings.outputErrorSpn = document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML; 
            settings.footerParagraph = document.getElementById(footerId);
            settings.footerText = settings.footerParagraph.innerHTML;
            try
            {
                settings.versionNo = electron.remote["app"].getVersion();
                settings.footerParagraph.innerHTML = settings.footerText.format(settings.versionNo);
            }
            catch(err) 
            {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Version.initialize"); 
            } 
        }
    }
}(jQuery);