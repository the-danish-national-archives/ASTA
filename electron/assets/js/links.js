/*
    Model is responsible for documents links
    inputs array of tag <a> ids
    <a> href attribute will be used as target document name
    alle documents are saved at ./assets/documents/
*/
window.Rigsarkiv = window.Rigsarkiv || {},
    function (n) {
        const {shell} = require('electron');
        const fs = require('fs');
        const os = require('os');

        var settings = {
            outputErrorSpn: null,
            outputErrorText: null,
            documentPath: "./assets/documents/{0}"
        }

        var GetPath = function(fileName) {
            var rootPath = null;
            if(os.platform() == "win32") {
                var folders =  __dirname.split("\\");
                rootPath = folders.slice(0,folders.length - 1).join("\\");
                result = "{0}\\documents\\{1}".format(rootPath,fileName);
                if(!fs.existsSync(settings.documentPath.format(fileName))) {
                    rootPath = folders.slice(0,folders.length - 3).join("\\");
                    result = "{0}\\{1}".format(rootPath,fileName);
                }
            }
            if(os.platform() == "darwin") {                
                var folders =  __dirname.split("/");
                rootPath = folders.slice(0,folders.length - 1).join("/");
                result = "{0}/documents/{1}".format(rootPath,fileName);
                if(!fs.existsSync(settings.documentPath.format(fileName))) {
                    rootPath = folders.slice(0,folders.length - 3).join("/");
                    result = "{0}/{1}".format(rootPath,fileName);
                }
            }           
            return result;
        }

        Rigsarkiv.Links = {
            initialize: function (outputErrorId,elemnetIds) {
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                var element = null;
                elemnetIds.forEach(elementId => {
                    element = document.getElementById(elementId);
                    if(element != null) {
                        element.addEventListener('click', (event) => {
                            var fileName = event.srcElement.href.split("#")[1];
                            shell.openItem(GetPath(fileName));
                        });
                    }
                    else {
                        console.logInfo(`none exist elment with id: ${elementId}`,"Rigsarkiv.Links.initialize");
                    }  
                });
            }
        }
}(jQuery);