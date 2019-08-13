/*
    Model is responsible for version (admin, none admin)
    initialize interface inputs: 
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const fs = require('fs');
    const path = require('path');
    const os = require('os');

    //private data members
    var settings = {
        outputErrorSpn: null,
        outputErrorText: null,
        footerParagraph: null,
        footerText: null,
        versionFileName: "version.json",
        scriptPath: "./assets/scripts/{0}",
        resourceWinPath: "resources\\{0}",
        footerInfo: null
    }

    // Ensure version Data
    var EnsureData = function() {
        var versionFilePath = settings.scriptPath.format(settings.versionFileName);        
        if(!fs.existsSync(versionFilePath)) {
            var rootPath = null;
            if(os.platform() == "win32") {
                rootPath = path.join('./');
                versionFilePath = path.join(rootPath,settings.resourceWinPath.format(settings.versionFileName));
            }
            if(os.platform() == "darwin") {
                var folders =  __dirname.split("/");
                rootPath = folders.slice(0,folders.length - 3).join("/");
                versionFilePath = "{0}/{1}".format(rootPath,settings.versionFileName);
            }
        }        
        console.logInfo(`read ${settings.versionFileName} file from: ${versionFilePath}`,"Rigsarkiv.Version.EnsureData");
        settings.footerInfo = JSON.parse(fs.readFileSync(versionFilePath));
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
                EnsureData();
                settings.footerParagraph.innerHTML = settings.footerText.format(settings.footerInfo.publishYear,settings.footerInfo.versionNo);
            }
            catch(err) 
            {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Version.initialize"); 
            } 
        }
    }
}(jQuery);