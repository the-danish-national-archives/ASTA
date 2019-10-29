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
        const domParser = require('xmldom');
        const deliveryPackagePattern = /^(FD.[1-9]{1}[0-9]{4,})$/;
        const dataTablePattern = /^(table[1-9]{1}([0-9]{0,}))$/;
        const dataFilePattern = /^(table[1-9]{1}([0-9]{0,}).csv)$/;
        const metadataFilePattern = /^(table[1-9]{1}([0-9]{0,}).txt)$/;
        const docFolderPattern = /^([1-9]{1}([0-9]{0,11}))$/;
        const errorsMax = 40;

        //private data memebers
        var settings = { 
            outputErrorSpn: null,
            outputErrorText: null,
            selectDirBtn: null,
            pathDirTxt: null,
            selectedPath: null,
            validateBtn: null,
            output: null,
            logCallback: null,
            metadataCallback: null,
            deliveryPackagePath: null,
            testSpn: null,
            confirmationSpn: null,
            outputText: {},
            metadataFileName: "{0}.txt",
            dataFileName: "{0}.csv",
            docCollectionFolderName: "docCollection1",
            docFilesExt: [".tif",".mpg",".mp3",".jpg","jp2"],
            defaultSubFolders: ["ContextDocumentation","Data","Indices"],
            defaultIndicesFiles: ["archiveIndex.xml","contextDocumentationIndex.xml"],
            defaultFolder: "FD.99999",
            logType: "structure",
            errors: 0,
            errorStop: false,
            convertStop: false,
            documents: []            
        }

        //reset status & input fields
        var Reset = function () {
            settings.ConvertBtn.hidden = true;
            settings.errors = 0;
            settings.errorStop = false;
            settings.convertStop = false;
            settings.documents = [];
            settings.output.html("");
        }

        // get selected folder name 
        var GetFolderName = function() {
            var folders = settings.deliveryPackagePath.getFolders();
            return folders[folders.length - 1];
        }

        // View Element by id & return texts
        var ViewElement = function(id,formatText1,formatText2,formatText3,formatText4, formatText5, formatText6) {
            var result = Rigsarkiv.Language.callback().getValue(id);
            if(result == null) { return null; } 
            if(formatText1 != null) { 
                if(formatText2 != null) {
                    if(formatText3 != null) {
                        if(formatText4 != null){
                            if(formatText5 != null){
                                if(formatText6 != null){
                                    result = result.format(formatText1,formatText2,formatText3,formatText4,formatText5,formatText6);    
                                }
                                else {
                                    result = result.format(formatText1,formatText2,formatText3,formatText4,formatText5);
                                }
                            }
                            else {
                                result = result.format(formatText1,formatText2,formatText3,formatText4);
                            }
                        }
                        else {
                            result = result.format(formatText1,formatText2,formatText3);
                        }
                    }
                    else {
                        result = result.format(formatText1,formatText2);
                    }
                }
                else {
                    result = result.format(formatText1);
                } 
            }
            if(result != null) {
                settings.output.html(settings.output.html() + "<span>{0}</span>".format(result));
            }
            return result;
        }

        //handle error logging + HTML output
        var LogError = function(id) {
            if(settings.errors < errorsMax) {
                var text = null;
                if (arguments.length > 1) {                
                    if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                    if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                    if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
                    if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4]); }
                    if(arguments.length === 6) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5]); }
                    if(arguments.length === 7) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5],arguments[6]); }
                }
                if(text != null) { settings.logCallback().error(settings.logType,GetFolderName(),text); }
            }
            settings.errors += 1;
            return false;
        }

        //Handle warn logging
        var LogWarn = function(id) {
            var text = null;
            if (arguments.length > 1) {                
                if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4]); }
                if(arguments.length === 6) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5]); }
                if(arguments.length === 7) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5],arguments[6]); }
            }
            if(text != null) { settings.logCallback().warn(settings.logType,GetFolderName(),text); }
            return true;
        }
        
        //Handle info logging
        var LogInfo = function(id) {
           var text = null;
            if (arguments.length > 1) {                
                if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4]); }
                if(arguments.length === 6) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5]); }
                if(arguments.length === 7) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5],arguments[6]); }
            }
            if(text != null) { settings.logCallback().info(settings.logType,GetFolderName(),text); }
            return true;
        }

        //Validate Indices contextDocumentationIndex.xml file
        var ValidateContextDocumentationIndex = function (destPath) {
            var result = true;
            var documentIds = [];
            var fileName =settings.defaultIndicesFiles[1];
            var filePath = (destPath.indexOf("\\") > -1) ? "{0}\\{1}".format(destPath,fileName) : "{0}/{1}".format(destPath,fileName);
            var data = fs.readFileSync(filePath);
            if(data != null) {
                data = data.toString();
                if(data != null && data !== "") {
                    var doc = new domParser.DOMParser().parseFromString(data);
                    for(var i = 0; i < doc.documentElement.childNodes.length;i++) {
                        var node = doc.documentElement.childNodes[i];
                        if(node.nodeName === "document" && node.childNodes != null && node.childNodes[1].nodeName === "documentID") {
                            documentIds.push(node.childNodes[1].firstChild.data);
                        }
                    }
                    documentIds.forEach(id => {
                        if(!settings.documents.includes(id)) {
                            result = LogError("nemesis-processing-CheckFolderIndices-ContextDocumentation-FileRequired-Error",fileName,id); 
                        }
                    });
                    settings.documents.forEach(id => {
                        if(!documentIds.includes(id)) {
                            result = LogError("nemesis-processing-CheckFolderContextDocumentation-ContextDocumentation-FileRequired-Error",fileName,id); 
                        }
                    });
                }
                else {
                    result = LogError("nemesis-processing-CheckFolderIndices-FileEmpty-Error",fileName);
                }
            }
            else {
                result = LogError("nemesis-processing-CheckFolderIndices-FileEmpty-Error",fileName);
            }
            return result;
        }

        //Validate Indices folder's files
        var ValidateIndices = function () {
            var result = true;
            var destPath = settings.deliveryPackagePath;
            destPath += (destPath.indexOf("\\") > -1) ? "\\{0}".format(settings.defaultSubFolders[2]) : "/{0}".format(settings.defaultSubFolders[2]); 
            var subFiles = fs.readdirSync(destPath);
            settings.defaultIndicesFiles.forEach(file => {
                if(!subFiles.includes(file)) { 
                    result = LogError("nemesis-processing-CheckFolderIndices-Error",file);
                }
            });
            if(result && subFiles.length > 2) {
                subFiles.forEach(file => {
                    if(result && file !== settings.defaultIndicesFiles[0] && file !== settings.defaultIndicesFiles[1]) {
                        result = LogError("nemesis-processing-CheckFolderIndicesCount-Error",file);
                    }
                });
            }
            if(result && !ValidateContextDocumentationIndex(destPath)) { result = false; }
            return result;
        }

        //validate wrong files
        var ValidateTableFiles = function(tableFolderName,subFiles) {
            var result = true;
            subFiles.forEach(file => {
                if(file !== settings.dataFileName.format(tableFolderName) && file !== settings.metadataFileName.format(tableFolderName)) {
                    var fileExt = file.substring(file.indexOf("."));
                    if(fileExt !== settings.dataFileName.format("") && fileExt !== settings.metadataFileName.format("")) {
                        result = LogError("nemesis-processing-CheckFolderData-TableFolderFileExt-Error",tableFolderName,file);
                    }
                    else {
                        if(dataFilePattern.test(file) || metadataFilePattern.test(file)) {
                            result = LogError("nemesis-processing-CheckFolderData-TableFolderFileOrder-Error",tableFolderName,file);
                            settings.errorStop = true;
                        }
                        else {
                            result = LogError("nemesis-processing-CheckFolderData-TableFolderFile-Error",tableFolderName,file);
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
                    result = LogError("nemesis-processing-CheckFolderData-TableFolderDataFile-Error",tableFolderName);
                }
                fileName = settings.metadataFileName.format(tableFolderName);
                if(!subFiles.includes(fileName)) {
                    result = LogError("nemesis-processing-CheckFolderData-TableFolderMetadataFile-Error",tableFolderName);
                }
            }
            else {
                result = LogError("nemesis-processing-CheckFolderData-TableFolderEmpty-Error",tableFolderName);
            }
            if(!result) { settings.errorStop = true; }
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
                result = LogError("nemesis-processing-CheckFolderData-TableFolders1-Error",null);
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
                result = LogError("nemesis-processing-CheckFolderData-TableFoldersOrder-Error",null);
            }
            return result;
        }

        //Validate Data folder & sub table datasets
        var ValidateData = function() {
            var result = true;
            var destPath = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.deliveryPackagePath,settings.defaultSubFolders[1]) : "{0}/{1}".format(settings.deliveryPackagePath,settings.defaultSubFolders[1]); 
            var subFolders = fs.readdirSync(destPath);
            if(subFolders != null && subFolders.length > 0) {
                var validTablesName = true;
                subFolders.filter(junk.not).forEach(folder => {
                    if(!dataTablePattern.test(folder)) {
                        result = LogError("nemesis-processing-CheckFolderData-TableFolders-Error",folder);
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
                            if(subFiles.filter(junk.not).length > 2) { 
                                result = LogError("nemesis-processing-CheckFolderData-TableFolderFilesCount-Error",folder);
                                settings.convertStop = true; 
                            }
                        }
                        if(!ValidateTableFiles(folder,subFiles.filter(junk.not))) { result = false; }                            
                    }
                });
                if(validTablesName && !ValidateTablesOrder(subFolders.filter(junk.not))) { result = false; }
            }
            else {
                settings.errorStop = true;
                result = LogError("nemesis-processing-CheckFolderDataEmpty-Error",null);
            }
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
                result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionDocumentFileName1-Error",documentFolderName);
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
                result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionDocumentFilesOrder-Error",documentFolderName);
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
                        result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionDocumentFileName-Error",documentFolderName,file);
                        validFilesName = false;
                    }
                    else {
                        var fileExt = file.substring(file.indexOf("."));
                        if(!settings.docFilesExt.includes(fileExt)) {
                            result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionDocumentFileExt-Error",documentFolderName,fileExt);
                        } 
                        else {
                            if(!filesExt.includes(fileExt)) { filesExt.push(fileExt); }
                        }
                    }                    
                });
                if(filesExt.length > 1) {
                    result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionDocumentFilesExt-Error",documentFolderName);
                }
                if(validFilesName && !ValidateDocumentFilesOrder(documentFolderName,subFiles)) { result = false; }
            }
            else {
                result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionDocumentFolderEmpty-Error",documentFolderName);
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
                result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionDocumentFolder1-Error",null);
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
                result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionDocumentFolderOrder-Error",null);
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
                subFolders.filter(junk.not).forEach(folder => {
                    settings.documents.push(folder);
                    if(!docFolderPattern.test(folder)) {
                        result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionDocumentFolder-Error",folder);
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
                if(validFoldersName && !ValidateDocumentFoldersOrder(subFolders.filter(junk.not))) { result = false; }
            }
            else {
                result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionEmpty-Error",null);
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
                    result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollections-Error",null);
                }
                else 
                {
                    if(!ValidateDocCollection(destPath)) {
                        result = false;
                    }
                    subFolders.filter(junk.not).forEach(folder => {
                        if(folder != settings.docCollectionFolderName) {
                            result = LogError("nemesis-processing-CheckFolderContextDocumentation-DocCollectionsCount-Error",folder);
                        }
                    });
                }
            }
            else {
                result = LogError("nemesis-processing-CheckFolderContextDocumentationEmpty-Error",null);
            }
            return result;
        }

        // loop sub folders Structure
        var ValidateStructure = function () {
            var result = true;
            var checkfolders = true;
            var subFolders = fs.readdirSync(settings.deliveryPackagePath);
            settings.defaultSubFolders.forEach(folder => {
                if(!subFolders.includes(folder)) {
                   result = LogError("nemesis-processing-CheckFolders-Error",folder);
                   checkfolders = false; 
                   settings.convertStop = true;
                   if(folder === settings.defaultSubFolders[1]) { settings.errorStop = true; }
                }
                else {
                    if(folder === settings.defaultSubFolders[0]) { 
                        if(!ValidateContextDocumentation()) { result = false; } 
                    }
                    if(folder === settings.defaultSubFolders[1]) { 
                        if(!ValidateData()) { result = false; } 
                    }
                    if(folder === settings.defaultSubFolders[2]) { 
                        if(!ValidateIndices()) { result = false; }
                    }
                }
            });
            if(checkfolders && subFolders.length > 3) {
                subFolders.filter(junk.not).forEach(folder => {
                    if(checkfolders && folder !== settings.defaultSubFolders[0] && folder !== settings.defaultSubFolders[1] && folder !== settings.defaultSubFolders[2]) {
                        result = LogError("nemesis-processing-CheckFoldersCount-Error",folder);
                    }
                });
            }
            return result;
        }

        //validate folder Name
        var ValidateName = function () {
            var result = true;
            var folderName = GetFolderName();
            if(!deliveryPackagePattern.test(folderName)) {
                result = LogError("nemesis-processing-CheckId-Error",null);
            }
            else {
                if(folderName === settings.defaultFolder) {
                    LogWarn("nemesis-processing-CheckId-Warning",null)
                }          
            }
            return result;    
        }

        //start flow validation
        var Validate = function() {            
            try 
            {
                var folderName = GetFolderName();
                ValidateName();
                ValidateStructure();
                 if(settings.errors === 0) {
                    LogInfo("nemesis-processing-CheckFolders-Ok",null);
                    settings.logCallback().section(settings.logType,folderName,Rigsarkiv.Language.callback().getValue("nemesis-output-structure-logEndNoError"));
                } else {
                    LogInfo(!settings.errorStop ? "nemesis-processing-CheckFolders-Warning" : "nemesis-processing-CheckFolders-ErrorStop",null);
                    settings.logCallback().section(settings.logType,folderName,!settings.errorStop ? Rigsarkiv.Language.callback().getValue("nemesis-output-structure-logEndWithError") : Rigsarkiv.Language.callback().getValue("nemesis-output-structure-logEndWithErrorStop"));                    
                }
                if(!settings.errorStop) { //TODO remove settings.outputText
                    return settings.metadataCallback().validate(settings.deliveryPackagePath,settings.outputText,settings.errors,settings.convertStop); 
                } 
                else {
                    settings.confirmationSpn.innerHTML = Rigsarkiv.Language.callback().getValue("nemesis-output-ConvertDisabled");
                    settings.selectDirBtn.disabled = false;
                    settings.validateBtn.disabled = false;
                    return settings.logCallback().commit(settings.deliveryPackagePath);
                }               
            }
            catch(err) 
            {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Nemesis.Structure.Validate");
            }
            return null;
        }

        var UpdatePath = function(path) {
            settings.ConvertBtn.hidden = true;
            settings.selectedPath = path; 
            console.logInfo(`selected path: ${path}`,"Rigsarkiv.Nemesis.Structure.AddEvents"); 
            settings.pathDirTxt.value = settings.selectedPath;
            settings.logCallback().reset();
            settings.confirmationSpn.innerHTML = "";
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {            
            settings.validateBtn.addEventListener('click', (event) => {
                Reset();
                if(settings.selectedPath == null || settings.pathDirTxt.value === "") { return; } 
                settings.logCallback().spinnerEnable(true);
                settings.selectDirBtn.disabled = true;
                settings.validateBtn.disabled = true;
                var folderName = GetFolderName();
                settings.testSpn.innerText = Rigsarkiv.Language.callback().getValue("nemesis-test-id").format(folderName);
                settings.logCallback().section(settings.logType,folderName,Rigsarkiv.Language.callback().getValue("nemesis-output-structure-logStart"));
                setTimeout(Validate, 1000);                          
            })
            settings.selectDirBtn.addEventListener('click', (event) => {
                ipcRenderer.send('validation-open-file-dialog');
            })
            ipcRenderer.on('validation-selected-directory', (event, path) => {
                UpdatePath(path);
                settings.deliveryPackagePath = settings.selectedPath[0];
            })            
        }

        //Model interfaces functions
        Rigsarkiv.Nemesis.Structure = {        
            initialize: function (logCallback,metadataCallback,outputErrorId,selectDirectoryId,pathDirectoryId,validateId,outputId,testId,confirmationId,convertId) {            
                settings.logCallback = logCallback;
                settings.metadataCallback = metadataCallback;
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.selectDirBtn = document.getElementById(selectDirectoryId);
                settings.pathDirTxt = document.getElementById(pathDirectoryId);
                settings.validateBtn = document.getElementById(validateId);
                settings.output = $("#" + outputId);
                settings.testSpn = document.getElementById(testId);
                settings.confirmationSpn = document.getElementById(confirmationId);
                settings.ConvertBtn = document.getElementById(convertId);
                //TODO remove
                $("span[id^='nemesis-processing']").each(function() {
                    settings.outputText[this.id] = $(this).html();
                    $(this).html("");
                });
                AddEvents();
            },
            callback: function () {
                return { 
                    validate: function(path) 
                    { 
                        settings.logCallback().spinnerEnable(false);
                        settings.deliveryPackagePath = path;
                        Reset();
                        settings.selectDirBtn.disabled = true;
                        settings.validateBtn.disabled = true;
                        var folderName = GetFolderName();
                        settings.testSpn.innerText = Rigsarkiv.Language.callback().getValue("nemesis-test-id").format(folderName);
                        settings.logCallback().section(settings.logType,folderName,Rigsarkiv.Language.callback().getValue("nemesis-output-structure-logStart"));
                        return Validate();
                    },
                    update: function(path)
                    {
                        UpdatePath(path);
                        settings.deliveryPackagePath = path;
                    }  
                };
            }
        };    
    }(jQuery);
}(jQuery);