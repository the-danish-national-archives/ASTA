window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron')
    const fs = require('fs');
    const pattern = /^(FD.[1-9]{1}[0-9]{4,})$/;

    var settings = { 
        selectDirBtn: null,
        pathDirTxt: null,
        selectedPath: null,
        validateBtn: null,
        outputPrefix: null,
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
            result = false;
        }
        else {
            if(folderName === settings.defaultFolder) {
                element = $("span#" + settings.outputPrefix + "-CheckId-Warning");
                element.show();
            }
            else {
                element = $("span#" + settings.outputPrefix + "-CheckId-Ok");
                element.show();
            }            
        }
        element.html(element.html().format(folderName))
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
         })
    }

    Rigsarkiv.Validation = {        
        initialize: function (selectDirectoryId,pathDirectoryId,validateId,outputPrefix) {            
            settings.selectDirBtn =  document.getElementById(selectDirectoryId);
            settings.pathDirTxt =  document.getElementById(pathDirectoryId);
            settings.validateBtn =  document.getElementById(validateId);
            settings.outputPrefix = outputPrefix;
            AddEvents();
        }
    };

}(jQuery);