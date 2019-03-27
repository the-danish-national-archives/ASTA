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
        const deliveryPackagePattern = /^(FD.[1-9]{1}[0-9]{4,})$/;
        const dataTablePattern = /^(table[0-9]{1,})$/;

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
            metadataFileName: "{0}.txt",
            dataFileName: "{0}.csv",
            defaultSubFolders: ["ContextDocumentation","Data","Indices"],
            defaultIndicesFiles: ["archiveIndex.xml","contextDocumentationIndex.xml"],
            defaultFolder: "FD.99999",
            logType: "structure",
            errorsCounter: 0,
            errorStop: false
        }

        //reset status & input fields
        var Reset = function () {
            settings.errorsCounter = 0;
            settings.errorStop = false;
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

        // View Element by id & return texts
        var ViewElement = function(id,formatText) {
            var result = settings.outputText[id];
            if(formatText != null) {
                result = result.format(formatText)
            }

            var element = $("span#{0}".format(id));            
            if(result != null) {
                element.html(element.html() + result);
            }
            element.show();

            return result;
        }

        //handle error logging + HTML output
        var LogError = function(postfixId,formatText) {
            var id = "{0}{1}".format(settings.outputPrefix,postfixId);
            var text = ViewElement(id,formatText);

            settings.logCallback().error(settings.logType,GetFolderName(),text);
            settings.errorsCounter += 1;
            return false;
        }

        //Handle warn logging
        var LogWarn = function(postfixId,formatText) {
            var id = "{0}{1}".format(settings.outputPrefix,postfixId);
            var text = ViewElement(id,formatText);

            settings.logCallback().warn(settings.logType,GetFolderName(),text);
        }
        
        //Handle info logging
        var LogInfo = function(postfixId,formatText) {
            var id = "{0}{1}".format(settings.outputPrefix,postfixId);
            var text = ViewElement(id,formatText);

            settings.logCallback().info(settings.logType,GetFolderName(),text);
        }

        //Validate Indices folder's files
        var ValidateIndices = function () {
            var result = true;
            var destPath = settings.deliveryPackagePath;
            destPath += (destPath.indexOf("\\") > -1) ? "\\{0}".format(settings.defaultSubFolders[2]) : "/{0}".format(settings.defaultSubFolders[2]); 
            var subFiles = fs.readdirSync(destPath);
            settings.defaultIndicesFiles.forEach(file => {
                if(!subFiles.includes(file)) { 
                    result = LogError("-CheckFolderIndices-Error",file);
                }
            });
            if(result && subFiles.length > 2) {
                subFiles.forEach(file => {
                    if(result && file !== settings.defaultIndicesFiles[0] && file !== settings.defaultIndicesFiles[1]) {
                        result = LogError("-CheckFolderIndicesCount-Error",file);
                    }
                });
            }
            if(result) {
                LogInfo("-CheckFolderIndices-Ok",null);
            }
            return result;
        }

        var ValidateTable = function(tableFolderName) {
            var result = true;
            var destPath = settings.deliveryPackagePath;
            destPath += (destPath.indexOf("\\") > -1) ? "\\{0}\\{1}".format(settings.defaultSubFolders[1],tableFolderName) : "/{0}/{1}".format(settings.defaultSubFolders[1],tableFolderName); 
            var subFiles = fs.readdirSync(destPath);
            if(subFiles != null && subFiles.length > 0) {
                var fileName = settings.dataFileName.format(tableFolderName);
                if(!subFiles.includes(fileName)) {
                    result = LogError("-CheckFolderData-TableFolderDataFile-Error",tableFolderName);
                }
                fileName = settings.metadataFileName.format(tableFolderName);
                if(!subFiles.includes(fileName)) {
                    result = LogError("-CheckFolderData-TableFolderMetadataFile-Error",tableFolderName);
                }
            }
            else {
                result = LogError("-CheckFolderData-TableFolderEmpty-Error",tableFolderName);
            }
            return result;
        }

        //Validate tables orders names
        var ValidateTablesOrder = function(subFolders) {
            var result = true;
            var tables = [];
            subFolders.forEach(folder => {
                tables.push(parseInt(folder.substring(5)));
            });
            tables.sort(function(a, b){return a-b});
            if(tables[0] !== 1) {
                result = LogError("-CheckFolderData-TableFolders1-Error",null);
            }
            var i;
            var orderResult = true;
            for (i = 0; i < tables.length; i++) { 
                if((i + 1) !== tables[i]) {
                    orderResult = false;
                    break;
                }
            }
            if(!orderResult) {
                result = LogError("-CheckFolderData-TableFoldersOrder-Error",null);
            }
            return result;
        }

        //Validate Data folder & sub table datasets
        var ValidateData = function() {
            var result = true;
            var destPath = settings.deliveryPackagePath;
            destPath += (destPath.indexOf("\\") > -1) ? "\\{0}".format(settings.defaultSubFolders[1]) : "/{0}".format(settings.defaultSubFolders[1]); 
            var subFolders = fs.readdirSync(destPath);
            if(subFolders != null && subFolders.length > 0) {
                id = "{0}-CheckFolderData-TableFolders-Error".format(settings.outputPrefix);
                settings.errorStop = true;
                subFolders.forEach(folder => {
                    if(!dataTablePattern.test(folder)) {
                        result = LogError("-CheckFolderData-TableFolders-Error",folder);
                    }//minimum one tableX
                    else 
                    { 
                        if(!ValidateTable(folder)) { result = false; }
                        settings.errorStop = false; 
                    }
                });
                if(!ValidateTablesOrder(subFolders)) { result = false; }
            }
            else {
                settings.errorStop = true;
                result = LogError("-CheckFolderDataEmpty-Error",null);
            }

            if(result) { LogInfo("-CheckFolderData-Ok",null); }
            return result;
        }

        var ValidateContextDocumentation = function() {
            var result = true;

            return result;
        }

        // loop sub folders Structure
        var ValidateStructure = function () {
            var result = true;
            var subFolders = fs.readdirSync(settings.deliveryPackagePath);
            settings.defaultSubFolders.forEach(folder => {
                if(!subFolders.includes(folder)) {
                   result = LogError("-CheckFolders-Error",folder);
                   if(folder === settings.defaultSubFolders[1]) { settings.errorStop = true; }
                }
                else {
                    if(folder === settings.defaultSubFolders[0]) { ValidateContextDocumentation(); }
                    if(folder === settings.defaultSubFolders[1]) { ValidateData(); }
                    if(folder === settings.defaultSubFolders[2]) { ValidateIndices(); }
                }
            });
            if(result && subFolders.length > 3) {
                subFolders.forEach(folder => {
                    if(result && folder !== settings.defaultSubFolders[0] && folder !== settings.defaultSubFolders[1] && folder !== settings.defaultSubFolders[2]) {
                        result = LogError("-CheckFoldersCount-Error",folder);
                    }
                });
            }
            
            if(result) { LogInfo("-CheckFolders-Ok",null); }
            return result;
        }

        //validate folder Name
        var ValidateName = function () {
            var result = true;
            var folderName = GetFolderName();
            if(!deliveryPackagePattern.test(folderName)) {
                result = LogError("-CheckId-Error",null);
            }
            else {
                if(folderName === settings.defaultFolder) {
                    LogWarn("-CheckId-Warning",null)
                }
                else {
                    LogInfo("-CheckId-Ok",null)
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
                } else {
                    settings.logCallback().section(settings.logType,folderName,settings.logEndWithErrorSpn.innerHTML);                    
                }
                if(!settings.errorStop) { 
                    settings.metadataCallback().validate(settings.deliveryPackagePath,settings.outputText); 
                } 
                else {
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