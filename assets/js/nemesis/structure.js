/*
    Model is responsible for Validate Delivery Package folder Structure (Flow 1.0)
    initialize interface inputs: elements from <div class="formularContainer">
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Nemesis = Rigsarkiv.Nemesis || {},
        function (n){
        const {ipcRenderer} = require('electron');
        const fs = require('fs');
        const pattern = /^(FD.[1-9]{1}[0-9]{4,})$/;

        //private data memebers
        var settings = { 
            selectDirBtn: null,
            pathDirTxt: null,
            selectedPath: null,
            validateBtn: null,
            outputPrefix: null,
            logCallback: null,
            metadataCallback: null,
            logStartSpn: null,
            logEndNoErrorSpn: null,
            logEndWithErrorSpn:null,
            deliveryPackagePath: null,
            outputText: {},
            defaultSubFolders: ["ContextDocumentation","Data","Indices"],
            defaultIndicesFiles: ["archiveIndex.xml","contextDocumentationIndex.xml"],
            defaultFolder: "FD.99999",
            logType: "structure",
            errorsCounter: 0
        }

        //reset status & input fields
        var Reset = function () {
            settings.errorsCounter = 0;
            $("span[id^='" + settings.outputPrefix + "']").hide();
             $("span[id^='" + settings.outputPrefix + "']").each(function() {
                $(this).html("");
            });
        }

        //output system error messages
        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputErrorSpn.hidden = false;
            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);
        }

        // get selected folder name 
        var GetFolderName = function() {
            var folders = settings.deliveryPackagePath.getFolders();
            return folders[folders.length - 1];
        }

        var ShowElement = function(id,text) {
            var element = $("span#{0}".format(id));            
            if(text != null) {
                element.html(element.html() + text);
            }
            element.show();
        }

        var ValidateIndices = function () {
            var result = true;
            var destPath = settings.deliveryPackagePath;
            destPath += (destPath.indexOf("\\") > -1) ? "\\{0}".format(settings.defaultSubFolders[2]) : "/{0}".format(settings.defaultSubFolders[2]); 
            var subFiles = fs.readdirSync(destPath);
            var folderName = GetFolderName();
            var id = "{0}-CheckFolderIndices-Error".format(settings.outputPrefix);
            settings.defaultIndicesFiles.forEach(file => {
                if(!subFiles.includes(file)) { 
                    ShowElement(id,settings.outputText[id].format(file));                           
                    settings.logCallback().error(settings.logType,folderName,settings.outputText[id].format(file));
                    settings.errorsCounter += 1;
                    result = false;
                }
            });
            if(result && subFiles.length > 2) {
                id = "{0}-CheckFolderIndicesCount-Error".format(settings.outputPrefix);
                subFiles.forEach(file => {
                    if(result && file !== settings.defaultIndicesFiles[0] && file !== settings.defaultIndicesFiles[1]) {
                        ShowElement(id,settings.outputText[id].format(file));
                        settings.logCallback().error(settings.logType,folderName,settings.outputText[id].format(file));
                        settings.errorsCounter += 1;
                        result = false;
                    }
                });
            }
            if(result) {
                id = "{0}-CheckFolderIndices-Ok".format(settings.outputPrefix);
                ShowElement(id,settings.outputText[id]);
                settings.logCallback().info(settings.logType,folderName,settings.outputText[id]);                
            }
        }

        var ValidateData = function() {

        }

        var ValidateContextDocumentation = function() {

        }

        // loop sub folders Structure
        var ValidateStructure = function () {
            var result = true;
            var subFolders = fs.readdirSync(settings.deliveryPackagePath);
            var folderName = GetFolderName();
            var id = "{0}-CheckFolders-Error".format(settings.outputPrefix);
            settings.defaultSubFolders.forEach(folder => {
                if(!subFolders.includes(folder)) {
                    ShowElement(id,settings.outputText[id].format(folder));
                    settings.logCallback().error(settings.logType,folderName,settings.outputText[id].format(folder));
                    settings.errorsCounter += 1;
                    result = false;
                }
                else {
                    switch(folder) {
                        case "ContextDocumentation": ValidateContextDocumentation(); break;
                        case "Data": ValidateData(); break;
                        case "Indices": ValidateIndices(); break;
                    }
                }
            });
            if(result && subFolders.length > 3) {
                id = "{0}-CheckFoldersCount-Error".format(settings.outputPrefix); 
                subFolders.forEach(folder => {
                    if(result && folder !== settings.defaultSubFolders[0] && folder !== settings.defaultSubFolders[1] && folder !== settings.defaultSubFolders[2]) {
                        ShowElement(id,settings.outputText[id].format(folder));
                        settings.logCallback().error(settings.logType,folderName,settings.outputText[id].format(folder));
                        settings.errorsCounter += 1;
                        result = false;
                    }
                });
            }
            if(result) {  
                id = "{0}-CheckFolders-Ok".format(settings.outputPrefix);                   
                ShowElement(id,settings.outputText[id]);
                settings.logCallback().info(settings.logType,folderName,settings.outputText[id]);
            }
            return result;
        }

        //validate folder Name
        var ValidateName = function () {
            var result = true;
            var id = null;
            var folderName = GetFolderName();            
            if(!pattern.test(folderName)) {
                id = "{0}-CheckId-Error".format(settings.outputPrefix);
                ShowElement(id,settings.outputText[id]);
                settings.logCallback().error(settings.logType,folderName,settings.outputText[id]);
                settings.errorsCounter += 1;
                result = false;
            }
            else {
                if(folderName === settings.defaultFolder) {
                    id = "{0}-CheckId-Warning".format(settings.outputPrefix);
                    ShowElement(id,settings.outputText[id]);
                    settings.logCallback().warn(settings.logType,folderName,settings.outputText[id]);
                }
                else {
                    id = "{0}-CheckId-Ok".format(settings.outputPrefix);
                    ShowElement(id,settings.outputText[id]);
                    settings.logCallback().info(settings.logType,folderName,settings.outputText[id]);
                }            
            }
            return result;    
        }

        //start flow validation
        var Validate = function() {
            try 
            {
                var folderName = GetFolderName();
                settings.logCallback().section(settings.logType,folderName,settings.logStartSpn.innerHTML);            
                ValidateName();
                ValidateStructure();
                if(settings.errorsCounter === 0) {
                    settings.logCallback().section(settings.logType,folderName,settings.logEndNoErrorSpn.innerHTML);
                    settings.metadataCallback().validate(settings.deliveryPackagePath,settings.outputText);                    
                } else {
                    settings.logCallback().section(settings.logType,folderName,settings.logEndWithErrorSpn.innerHTML);
                    settings.logCallback().commit(settings.deliveryPackagePath);
                }                
            }
            catch(err) 
            {
                HandleError(err);
            }
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.validateBtn.addEventListener('click', (event) => {
                Reset();
                if(settings.selectedPath == null || settings.pathDirTxt.value === "") { return; }                
                settings.deliveryPackagePath = settings.selectedPath[0];
                Validate();                           
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
            initialize: function (logCallback,metadataCallback,outputErrorId,selectDirectoryId,pathDirectoryId,validateId,logStartId,logEndNoErrorId,logEndWithErrorId,outputPrefix) {            
                settings.logCallback = logCallback;
                settings.metadataCallback = metadataCallback;
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.selectDirBtn = document.getElementById(selectDirectoryId);
                settings.pathDirTxt = document.getElementById(pathDirectoryId);
                settings.validateBtn = document.getElementById(validateId);
                settings.logStartSpn = document.getElementById(logStartId);
                settings.logEndNoErrorSpn = document.getElementById(logEndNoErrorId);  
                settings.logEndWithErrorSpn = document.getElementById(logEndWithErrorId);
                settings.outputPrefix = outputPrefix;
                $("span[id^='" + settings.outputPrefix + "']").each(function() {
                    settings.outputText[this.id] = $(this).html();
                    $(this).html("");
                });
                AddEvents();
            }
        };    
    }(jQuery);
}(jQuery);