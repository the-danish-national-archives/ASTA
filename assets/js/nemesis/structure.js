window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Nemesis = Rigsarkiv.Nemesis || {},
        function (n){
        const {ipcRenderer} = require('electron')
        const {shell} = require('electron')
        const fs = require('fs');
        const pattern = /^(FD.[1-9]{1}[0-9]{4,})$/;

        var settings = { 
            selectDirBtn: null,
            pathDirTxt: null,
            selectedPath: null,
            selectedLogFilePath: null,
            validateBtn: null,
            outputErrorSpn: null,
            outputErrorText: null,
            outputOkSpn: null,
            outputOkText: null,
            selectLogfile: null,
            outputPrefix: null,
            logCallback: null,
            defaultFolder: "FD.99999"
        }

        var Reset = function () {
            settings.outputErrorSpn.hidden = true;
            settings.outputOkSpn.hidden = true;
            settings.selectLogfile.hidden = true;
            $("span[id^='" + settings.outputPrefix + "']").hide();
        }

        var ShowOutput = function() {
            settings.selectedLogFilePath = settings.logCallback().commit(settings.selectedPath[0]);
            var folders = settings.selectedLogFilePath.normlizePath().split("/");
            var folderName = folders[folders.length - 1];
            settings.selectLogfile.innerHTML = settings.selectedLogFilePath + "]";
            settings.selectLogfile.hidden = false;
            settings.outputOkSpn.hidden = false;
            settings.outputOkSpn.innerHTML = settings.outputOkText.format(folderName);
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
            ShowOutput();            
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
            settings.selectLogfile.addEventListener('click', (event) => {
                shell.openItem(settings.selectedLogFilePath);
            });
        }

        Rigsarkiv.Nemesis.Structure = {        
            initialize: function (logCallback,selectDirectoryId,pathDirectoryId,validateId,outputErrorId,outputOkId,selectLogfileId,outputPrefix) {            
                settings.logCallback = logCallback;
                settings.selectDirBtn =  document.getElementById(selectDirectoryId);
                settings.pathDirTxt =  document.getElementById(pathDirectoryId);
                settings.validateBtn =  document.getElementById(validateId);
                settings.outputErrorSpn =  document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.outputOkSpn =  document.getElementById(outputOkId);
                settings.outputOkText = settings.outputOkSpn.innerHTML;
                settings.selectLogfile = document.getElementById(selectLogfileId);
                settings.outputPrefix = outputPrefix;
                AddEvents();
            }
        };    
    }(jQuery);
}(jQuery);