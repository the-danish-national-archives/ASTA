/*
    Model is responsible for rights (admin, none admin)
    initialize interface inputs: 
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const fs = require('fs');
    const path = require('path');
    const os = require('os');
    const { base64encode, base64decode } = require('nodejs-base64');

    //private data memebers
    var settings = {
        outputErrorSpn: null,
        outputErrorText: null,
        rightsFileName: "rights.json",
        scriptPath: "./assets/scripts/{0}",
        resourceWinPath: "resources\\{0}",
        rights: null,
        isAdmin: false,
        adminRight: "Rigsarkiv-Admin-20190401"
    }

    // Ensure rights Data
    var EnsureData = function() {
        var rightsFilePath = settings.scriptPath.format(settings.rightsFileName);        
        if(!fs.existsSync(rightsFilePath)) {
            var rootPath = null;
            if(os.platform() == "win32") {
                rootPath = path.join('./');
                rightsFilePath = path.join(rootPath,settings.resourceWinPath.format(settings.rightsFileName));
            }
            if(os.platform() == "darwin") {
                var folders =  __dirname.split("/");
                rootPath = folders.slice(0,folders.length - 3).join("/");
                rightsFilePath = "{0}/{1}".format(rootPath,settings.rightsFileName);
            }
        }        
        console.log(`read ${settings.rightsFileName} file from: ${rightsFilePath}`);
        if(fs.existsSync(rightsFilePath)) {
            settings.rights = JSON.parse(fs.readFileSync(rightsFilePath));
        }         
    }

     //Model interfaces functions
     Rigsarkiv.Rights = {
        initialize: function (outputErrorId) {
            settings.outputErrorSpn = document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML; 
            //var encoded = base64encode(settings.adminRight);
            //console.log(`${encoded}`);
            try
            {
                EnsureData();
                if(settings.rights != null) {
                    settings.isAdmin = (base64decode(settings.rights.key) === settings.adminRight)
                }
            }
            catch(err) 
            {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText); 
            }            
        },
        callback: function () {
            return {
                isAdmin: settings.isAdmin
            }
        }
    }
}(jQuery);