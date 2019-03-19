window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Nemesis = Rigsarkiv.Nemesis || {},
        function (n){
        const {ipcRenderer} = require('electron')
        const pattern = /^(FD.[1-9]{1}[0-9]{4,})$/;

        var settings = { 
            selectDirBtn: null,
            pathDirTxt: null,
            selectedPath: null,
            validateBtn: null,
            outputPrefix: null,
            logCallback: null,
            defaultFolder: "FD.99999"
        }

        var Reset = function () {
            $("span[id^='" + settings.outputPrefix + "']").hide();
        }

        var ValidateStructure = function (folderName) {
            var result = true;
            var element = null;
            if(!pattern.test(folderName)) {
                element = $("span#" + settings.outputPrefix + "-CheckId-Error");            
                element.show();
                settings.logCallback().error(element.text().format(folderName));
                result = false;
            }
            else {
                if(folderName === settings.defaultFolder) {
                    element = $("span#" + settings.outputPrefix + "-CheckId-Warning");
                    element.show();
                    settings.logCallback().warn(element.text().format(folderName));
                }
                else {
                    element = $("span#" + settings.outputPrefix + "-CheckId-Ok");
                    element.show();
                    settings.logCallback().info(element.text().format(folderName));
                }            
            }
            element.html(element.html().format(folderName));
            settings.logCallback().commit(settings.selectedPath[0]);            
            return result;
        }

        var AddEvents = function () {
            settings.validateBtn.addEventListener('click', (event) => {
                Reset();
                if(settings.selectedPath == null || settings.pathDirTxt.value === "") { return; }
                var folders = settings.selectedPath[0].split("\\");
                var folderName = folders[folders.length - 1];
                if(!ValidateStructure(folderName)) { return; }            
            })
            settings.selectDirBtn.addEventListener('click', (event) => {
                ipcRenderer.send('validation-open-file-dialog');
            })
            ipcRenderer.on('validation-selected-directory', (event, path) => {
                settings.selectedPath = path; 
                console.log(`selected path: ${path}`); 
                settings.pathDirTxt.value = settings.selectedPath;
                settings.logCallback().info("selected path: {0}".format(settings.selectedPath[0]));
            })            
        }

        Rigsarkiv.Nemesis.Structure = {        
            initialize: function (logCallback,selectDirectoryId,pathDirectoryId,validateId,outputPrefix) {            
                settings.logCallback = logCallback;
                settings.selectDirBtn =  document.getElementById(selectDirectoryId);
                settings.pathDirTxt =  document.getElementById(pathDirectoryId);
                settings.validateBtn =  document.getElementById(validateId);
                settings.outputPrefix = outputPrefix;
                AddEvents();
            }
        };    
    }(jQuery);
}(jQuery);