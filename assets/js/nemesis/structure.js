/*
    Model is responsible for Validate Delivery Package folder Structure
    initialize interface inputs: elements from <div class="formularContainer">
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Nemesis = Rigsarkiv.Nemesis || {},
        function (n){
        const {ipcRenderer} = require('electron')
        const pattern = /^(FD.[1-9]{1}[0-9]{4,})$/;

        //private data memebers
        var settings = { 
            selectDirBtn: null,
            pathDirTxt: null,
            selectedPath: null,
            validateBtn: null,
            outputPrefix: null,
            logCallback: null,
            logStartSpn: null,
            logEndNoErrorSpn: null,
            logEndWithErrorSpn:null,
            defaultFolder: "FD.99999",
            logType: "structure"
        }

        //reset status & input fields
        var Reset = function () {
            $("span[id^='" + settings.outputPrefix + "']").hide();
        }

        //validate folder structure
        var ValidateStructure = function (folderName) {
            var ex = document.getElementById("nemesis-output-structure-Error");
            ex.innerHTML = "<br />Error: {0}".format(settings.selectedPath[0]);
            
            var result = true;
            var element = null;
            settings.logCallback().info(settings.logType,folderName,settings.logStartSpn.innerHTML);
            if(!pattern.test(folderName)) {
                element = $("span#" + settings.outputPrefix + "-CheckId-Error");            
                element.show();
                settings.logCallback().error(settings.logType,folderName,element.text().format(folderName));
                result = false;
            }
            else {
                if(folderName === settings.defaultFolder) {
                    element = $("span#" + settings.outputPrefix + "-CheckId-Warning");
                    element.show();
                    settings.logCallback().warn(settings.logType,folderName,element.text().format(folderName));
                }
                else {
                    element = $("span#" + settings.outputPrefix + "-CheckId-Ok");
                    element.show();
                    settings.logCallback().info(settings.logType,folderName,element.text().format(folderName));
                }            
            }
            element.html(element.html().format(folderName));
            if(result) {
                settings.logCallback().info(settings.logType,folderName,settings.logEndNoErrorSpn.innerHTML);
            } else {
                settings.logCallback().info(settings.logType,folderName,settings.logEndWithErrorSpn.innerHTML);
            }
            settings.logCallback().commit(settings.selectedPath[0]);            
            return result;
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.validateBtn.addEventListener('click', (event) => {
                Reset();
                if(settings.selectedPath == null || settings.pathDirTxt.value === "") { return; }
                var folders = settings.selectedPath[0].getFolders();
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

        //Model interfaces functions
        Rigsarkiv.Nemesis.Structure = {        
            initialize: function (logCallback,selectDirectoryId,pathDirectoryId,validateId,logStartId,logEndNoErrorId,logEndWithErrorId,outputPrefix) {            
                settings.logCallback = logCallback;
                settings.selectDirBtn = document.getElementById(selectDirectoryId);
                settings.pathDirTxt = document.getElementById(pathDirectoryId);
                settings.validateBtn = document.getElementById(validateId);
                settings.logStartSpn = document.getElementById(logStartId);
                settings.logEndNoErrorSpn = document.getElementById(logEndNoErrorId);  
                settings.logEndWithErrorSpn = document.getElementById(logEndWithErrorId);
                settings.outputPrefix = outputPrefix;
                AddEvents();
            }
        };    
    }(jQuery);
}(jQuery);