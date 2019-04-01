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
        const junk = require('junk');
        const deliveryPackagePattern = /^(FD.[1-9]{1}[0-9]{4,})$/;
        const dataTablePattern = /^(table[1-9]{1}([0-9]{0,}))$/;
        const dataFilePattern = /^(table[1-9]{1}([0-9]{0,}).csv)$/;
        const metadataFilePattern = /^(table[1-9]{1}([0-9]{0,}).txt)$/;
        const docFolderPattern = /^([1-9]{1}([0-9]{0,11}))$/;

        //private data memebers
        var settings = { 
            outputErrorSpn: null,
            outputErrorText: null,
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
            testId: null,
           outputText: {},
            metadataFileName: "{0}.txt",
            dataFileName: "{0}.csv",
            docCollectionFolderName: "docCollection1",
            docFilesExt: [".tif",".mpg",".mp3",".jpg","jp2"],
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
        var ViewElement = function(id,formatText1,formatText2,formatText3) {
            var result = settings.outputText[id];
            if(formatText1 != null) { 
                if(formatText2 != null) {
                    if(formatText3 != null) {
                        result = result.format(formatText1,formatText2,formatText3);
                    }
                    else {
                        result = result.format(formatText1,formatText2);
                    }
                }
                else {
                    result = result.format(formatText1);
                } 
            }

            var element = $("span#{0}".format(id));            
            if(result != null) {
                element.html(element.html() + result);
            }
            element.show();

            return result;
        }

        //handle error logging + HTML output
        var LogError = function(postfixId) {
            var id = "{0}{1}".format(settings.outputPrefix,postfixId);
            var text = null;
            if (arguments.length > 1) {                
                if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
            }

            settings.logCallback().error(settings.logType,GetFolderName(),text);
            settings.errorsCounter += 1;
            return false;
        }

        //Handle warn logging
        var LogWarn = function(postfixId) {
            var id = "{0}{1}".format(settings.outputPrefix,postfixId);
            var text = null;
            if (arguments.length > 1) {                
                if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
            }

            settings.logCallback().warn(settings.logType,GetFolderName(),text);
        }
        
        //Handle info logging
        var LogInfo = function(postfixId) {
            var id = "{0}{1}".format(settings.outputPrefix,postfixId);
            var text = null;
            if (arguments.length > 1) {                
                if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
            }

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

        //validate wrong files
        var ValidateTableFiles = function(tableFolderName,subFiles) {
            var result = true;
            subFiles.forEach(file => {
                if(file !== settings.dataFileName.format(tableFolderName) && file !== settings.metadataFileName.format(tableFolderName)) {
                    var fileExt = file.substring(file.indexOf("."));
                    if(fileExt !== settings.dataFileName.format("") && fileExt !== settings.metadataFileName.format("")) {
                        result = LogError("-CheckFolderData-TableFolderFileExt-Error",tableFolderName,file);
                    }
                    else {
                        if(dataFilePattern.test(file) || metadataFilePattern.test(file)) {
                            result = LogError("-CheckFolderData-TableFolderFileOrder-Error",tableFolderName,file);
                        }
                        else {
                            result = LogError("-CheckFolderData-TableFolderFile-Error",tableFolderName,file);
                        }
                    }
                }
            });
            return result;
        }

        //Validate table folder must exits files
        var ValidateTable = function(tableFolderName,subFiles) {
            var result = true;
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
            //minimum one tableX valid files
            if(result) { settings.errorStop = false; }
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
            var destPath = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.deliveryPackagePath,settings.defaultSubFolders[1]) : "{0}/{1}".format(settings.deliveryPackagePath,settings.defaultSubFolders[1]); 
            var subFolders = fs.readdirSync(destPath);
            if(subFolders != null && subFolders.length > 0) {
                settings.errorStop = true;
                var validTablesName = true;
                subFolders.forEach(folder => {
                    if(!dataTablePattern.test(folder)) {
                        result = LogError("-CheckFolderData-TableFolders-Error",folder);
                        validTablesName = false;
                    }
                    else 
                    {
                        var destTablePath = (destPath.indexOf("\\") > -1) ? "{0}\\{1}".format(destPath,folder) : "{0}/{1}".format(destPath,folder); 
                        var subFiles = fs.readdirSync(destTablePath);
                        if(!ValidateTable(folder,subFiles.filter(junk.not))) { 
                            result = false; 
                        }
                        else {
                            if(subFiles.length > 2) { result = LogError("-CheckFolderData-TableFolderFilesCount-Error",folder); }
                        }
                        if(!ValidateTableFiles(folder,subFiles)) { result = false; }                            
                    }
                });
                if(validTablesName && !ValidateTablesOrder(subFolders)) { result = false; }
            }
            else {
                settings.errorStop = true;
                result = LogError("-CheckFolderDataEmpty-Error",null);
            }
            if(result) { LogInfo("-CheckFolderData-Ok",null); }
            return result;
        }

        //Validate Document files orders
        var ValidateDocumentFilesOrder = function(documentFolderName,subFiles) {
            var result = true;
            var files = [];
            subFiles.forEach(file => {
                files.push(parseInt(file.substring(0,file.indexOf("."))));
            });
            files.sort(function(a, b){return a-b});
            if(files[0] !== 1) {
                result = LogError("-CheckFolderContextDocumentation-DocCollectionDocumentFileName1-Error",documentFolderName);
            }
            var i;
            var orderResult = true;
            for (i = 0; i < files.length; i++) { 
                if((i + 1) !== files[i]) {
                    orderResult = false;
                    break;
                }
            }
            if(!orderResult) {
                result = LogError("-CheckFolderContextDocumentation-DocCollectionDocumentFilesOrder-Error",documentFolderName);
            }
            return result;
        }

        //Validate Document Folder
        var ValidateDocumentFolder = function(documentFolderName,subFiles) {
            var result = true;
            if(subFiles != null && subFiles.length > 0) {
               var filesExt = [];
               var validFilesName = true;
                subFiles.forEach(file => {                    
                    var fileName = file.substring(0,file.indexOf("."))
                    if(!docFolderPattern.test(fileName)) {
                        result = LogError("-CheckFolderContextDocumentation-DocCollectionDocumentFileName-Error",documentFolderName,file);
                        validFilesName = false;
                    }
                    else {
                        var fileExt = file.substring(file.indexOf("."));
                        if(!settings.docFilesExt.includes(fileExt)) {
                            result = LogError("-CheckFolderContextDocumentation-DocCollectionDocumentFileExt-Error",documentFolderName,fileExt);
                        } 
                        else {
                            if(!filesExt.includes(fileExt)) { filesExt.push(fileExt); }
                        }
                    }                    
                });
                if(filesExt.length > 1) {
                    result = LogError("-CheckFolderContextDocumentation-DocCollectionDocumentFilesExt-Error",documentFolderName);
                }
                if(validFilesName && !ValidateDocumentFilesOrder(documentFolderName,subFiles)) { result = false; }
            }
            else {
                result = LogError("-CheckFolderContextDocumentation-DocCollectionDocumentFolderEmpty-Error",documentFolderName);
            }
            return result;   
        }

        //Validate docCollection1 folder orders
        var ValidateDocumentFoldersOrder = function(subFolders) {
            var result = true;
            var folders = [];
            subFolders.forEach(folder => {
                folders.push(parseInt(folder));
            });
            folders.sort(function(a, b){return a-b});
            if(folders[0] !== 1) {
                result = LogError("-CheckFolderContextDocumentation-DocCollectionDocumentFolder1-Error",null);
            }
            var i;
            var orderResult = true;
            for (i = 0; i < folders.length; i++) { 
                if((i + 1) !== folders[i]) {
                    orderResult = false;
                    break;
                }
            }
            if(!orderResult) {
                result = LogError("-CheckFolderContextDocumentation-DocCollectionDocumentFolderOrder-Error",null);
            }
            return result;
        }

        //Validate docCollection1 folder
        var ValidateDocCollection = function(contextDocumentationPath) {
            var result = true;
            var destPath = (contextDocumentationPath.indexOf("\\") > -1) ? "{0}\\{1}".format(contextDocumentationPath,settings.docCollectionFolderName) : "{0}/{1}".format(contextDocumentationPath,settings.docCollectionFolderName); 
            var subFolders = fs.readdirSync(destPath);
            if(subFolders != null && subFolders.length > 0) {
                var validFoldersName = true;
                subFolders.forEach(folder => {
                    if(!docFolderPattern.test(folder)) {
                        result = LogError("-CheckFolderContextDocumentation-DocCollectionDocumentFolder-Error",folder);
                        validFoldersName = false;
                    }
                    else {
                        var destFolderPath = (destPath.indexOf("\\") > -1) ? "{0}\\{1}".format(destPath,folder) : "{0}/{1}".format(destPath,folder); 
                        var subFiles = fs.readdirSync(destFolderPath);                        
                        if(!ValidateDocumentFolder(folder,subFiles.filter(junk.not))) {
                            result = false;
                        }
                    }
                });
                if(validFoldersName && !ValidateDocumentFoldersOrder(subFolders)) { result = false; }
            }
            else {
                result = LogError("-CheckFolderContextDocumentation-DocCollectionEmpty-Error",null);
            }
            return result;
        }

        //Validate ContextDocumentation folder
        var ValidateContextDocumentation = function() {
            var result = true;
            var destPath = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.deliveryPackagePath,settings.defaultSubFolders[0]) : "{0}/{1}".format(settings.deliveryPackagePath,settings.defaultSubFolders[0]); 
            var subFolders = fs.readdirSync(destPath);
            if(subFolders != null && subFolders.length > 0) {
                if(!subFolders.includes(settings.docCollectionFolderName)) {
                    result = LogError("-CheckFolderContextDocumentation-DocCollections-Error",null);
                }
                else 
                {
                    if(!ValidateDocCollection(destPath)) {
                        result = false;
                    }
                    subFolders.forEach(folder => {
                        if(folder != settings.docCollectionFolderName) {
                            result = LogError("-CheckFolderContextDocumentation-DocCollectionsCount-Error",folder);
                        }
                    });
                }
            }
            else {
                result = LogError("-CheckFolderContextDocumentationEmpty-Error",null);
            }
            if(result) { LogInfo("-CheckFolderContextDocumentation-Ok",null); }
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
                settings.testId.innerText = folderName;
                settings.logCallback().section(settings.logType,folderName,settings.logStartSpn.innerHTML);            
                ValidateName();
                ValidateStructure();
                if(settings.errorsCounter === 0) {
                    settings.logCallback().section(settings.logType,folderName,settings.logEndNoErrorSpn.innerHTML);
                } else {
                    settings.logCallback().section(settings.logType,folderName,settings.logEndWithErrorSpn.innerHTML);                    
                }
                if(!settings.errorStop) { 
                    return settings.metadataCallback().validate(settings.deliveryPackagePath,settings.outputText); 
                } 
                else {
                    return settings.logCallback().commit(settings.deliveryPackagePath);
                }                  
                                
            }
            catch(err) 
            {
                HandleError(err);
            }
            return null;
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
            initialize: function (logCallback,metadataCallback,outputErrorId,selectDirectoryId,pathDirectoryId,validateId,logStartId,logEndNoErrorId,logEndWithErrorId,outputPrefix,testId) {            
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
                settings.testId = document.getElementById(testId);
                $("span[id^='" + settings.outputPrefix + "']").each(function() {
                    settings.outputText[this.id] = $(this).html();
                    $(this).html("");
                });
                AddEvents();
            },
            callback: function () {
                return { 
                    validate: function(path) 
                    { 
                        settings.deliveryPackagePath = path;
                        Reset();
                        return Validate();
                    }  
                };
            }
        };    
    }(jQuery);
}(jQuery);