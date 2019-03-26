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
                $(this).html(settings.outputText[this.id]);
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
            var folders = settings.selectedPath[0].getFolders();
            return folders[folders.length - 1];
        }

        var ValidateIndices = function () {
            var result = true;
            var destPath = settings.selectedPath[0];
            destPath += (destPath.indexOf("\\") > -1) ? "\\{0}".format(settings.defaultSubFolders[2]) : "/{0}".format(settings.defaultSubFolders[2]); 
            var subFiles = fs.readdirSync(destPath);
            var folderName = GetFolderName();
            var element = $("span#" + settings.outputPrefix + "-CheckFolderIndices-Error");
            settings.defaultIndicesFiles.forEach(file => {
                if(!subFiles.includes(file)) {                            
                    element.show();
                    settings.logCallback().error(settings.logType,folderName,element.text().format(file));
                    element.html(element.html().format(file));
                    settings.errorsCounter += 1;
                    result = false;
                }
            });
            if(result && subFiles.length > 2) {
                element = $("span#" + settings.outputPrefix + "-CheckFolderIndicesCount-Error");
                subFiles.forEach(file => {
                    if(result && file !== settings.defaultIndicesFiles[0] && file !== settings.defaultIndicesFiles[1]) {
                        element.show();
                        settings.logCallback().error(settings.logType,folderName,element.text().format(file));
                        element.html(element.html().format(file));
                        settings.errorsCounter += 1;
                        result = false;
                    }
                });
            }
            if(result) {
                element = $("span#" + settings.outputPrefix + "-CheckFolderIndices-Ok");
                element.show(); 
                settings.logCallback().info(settings.logType,folderName,element.text());                
            }
        }

        var ValidateData = function() {

        }

        var ValidateContextDocumentation = function() {

        }

        // loop sub folders Structure
        var ValidateStructure = function () {
            var result = true;
            var subFolders = fs.readdirSync(settings.selectedPath[0]);
            var folderName = GetFolderName();
            var element = $("span#" + settings.outputPrefix + "-CheckFolders-Error");
            settings.defaultSubFolders.forEach(folder => {
                if(!subFolders.includes(folder)) {                            
                    element.show();
                    settings.logCallback().error(settings.logType,folderName,element.text().format(folder));
                    element.html(element.html().format(folder));
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
                element = $("span#" + settings.outputPrefix + "-CheckFoldersCount-Error");
                subFolders.forEach(folder => {
                    if(result && folder !== settings.defaultSubFolders[0] && folder !== settings.defaultSubFolders[1] && folder !== settings.defaultSubFolders[2]) {
                        element.show();
                        settings.logCallback().error(settings.logType,folderName,element.text().format(folder));
                        element.html(element.html().format(folder));
                        settings.errorsCounter += 1;
                        result = false;
                    }
                });
            }
            if(result) {                    
                element = $("span#" + settings.outputPrefix + "-CheckFolders-Ok");
                element.show(); 
                settings.logCallback().info(settings.logType,folderName,element.text());
            }
            return result;
        }

        //validate folder Name
        var ValidateName = function () {
            var result = true;
            var element = null;
            var folderName = GetFolderName();            
            if(!pattern.test(folderName)) {
                element = $("span#" + settings.outputPrefix + "-CheckId-Error");            
                element.show();
                settings.logCallback().error(settings.logType,folderName,element.text());
                settings.errorsCounter += 1;
                result = false;
            }
            else {
                if(folderName === settings.defaultFolder) {
                    element = $("span#" + settings.outputPrefix + "-CheckId-Warning");
                    element.show();
                    settings.logCallback().warn(settings.logType,folderName,element.text());
                }
                else {
                    element = $("span#" + settings.outputPrefix + "-CheckId-Ok");
                    element.show();
                    settings.logCallback().info(settings.logType,folderName,element.text());
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
                    settings.metadataCallback().validate(settings.selectedPath);                    
                } else {
                    settings.logCallback().section(settings.logType,folderName,settings.logEndWithErrorSpn.innerHTML);
                    settings.logCallback().commit(settings.selectedPath[0]);
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
                });
                AddEvents();
            }
        };    
    }(jQuery);
}(jQuery);