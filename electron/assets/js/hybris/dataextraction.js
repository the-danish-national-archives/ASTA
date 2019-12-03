/*
    Model is responsible for data extraction from statistics program
    initialize interface inputs: elements from <div id="hybris-panel-statistics">
    callback interface outputs: 
        structure Callback 
        data relative Folder Path: /Data/tableX
        selected Statistics File absolut Path
        script Type: SPSS, SAS, Stata
        local Folder absolut Path
    scripts templates at ./assets/scripts/
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
    function (n) {
        const {ipcRenderer} = require('electron');
        const fs = require('fs');
        const path = require('path');
        const chardet = require('chardet');
        const os = require('os');
        const MaxFileSize = 524288000;

        //private data memebers
        var settings = {
            scriptType: null,
            scriptApplication: null,
            scriptFileName: null,
            dataFolderPath: null,
            selectStatisticsFileBtn: null,
            pathStatisticsFileTxt: null,
            okStatisticsBtn: null,
            outputStructureOkSpn: null,
            selectStructureDeliveryPackage: null,
            outputStatisticsErrorSpn: null,
            outputStatisticsErrorText: null,
            outputStatisticsOkCopyScriptSpn: null,
            outputStatisticsOkCopyScriptInfoSpn: null,
            selectedStatisticsFilePath: null,
            outputScriptOkSpn: null,
            metadataFileName: null,
            scriptPanel1: null,
            scriptPanel2: null,  
            okScriptBtn: null,
            nextBtn: null, 
            spinner: null, 
            spinnerClass: null,
            metdataTab: null, 
            headerLinkTrin2: null,
            headerLinkTrin3: null,
            headerLinkInformation2: null,
            outputStatisticsSASPopup: null,
            okScriptDataPath: null,
            variablesDropdown: null,   
            scriptPath: "./assets/scripts/{0}",
            resourceWinPath: "resources\\{0}",
            scripts: ["spss_script.sps","sas_without_catalog_script.sas","sas_with_catalog_script.sas","stata_script.do"],
            outputPostfixFiles: ["{0}.csv","{0}_VARIABEL.txt","{0}_VARIABELBESKRIVELSE.txt"],
            outputOptionalPostfixFiles: ["{0}_KODELISTE.txt","{0}_BRUGERKODE.txt"],
            sasCatalogFileExt: "{0}.sas7bcat",
            dataPathPostfix: "Data",
            dataTablePathPostfix: "table{0}",
            variableFileName: null,
            scriptPathLink: null            
        }

        //reset status & input fields
        var Reset = function () { 
            settings.okScriptBtn.hidden = false;       
            settings.outputStatisticsErrorSpn.hidden = true;
            settings.scriptPanel1.hidden = false;
            settings.scriptPanel2.hidden = true;
            settings.outputScriptOkSpn.hidden = true;
            settings.nextBtn.hidden = true;
            settings.outputStructureOkSpn.hidden = true;
            settings.outputStatisticsSASPopup.hidden = true;
            settings.headerLinkTrin2.innerHTML = "";
            settings.headerLinkTrin3.innerHTML = "";
            settings.headerLinkInformation2.innerHTML = "";
        }

        //get selected Statistics File name
        var GetFileName = function() {
            var filePath = settings.selectedStatisticsFilePath[0];
            var folders = filePath.getFolders();
            return folders[folders.length - 1];
        }

        //get selected Statistics File path
        var GetFolderPath = function() {
            var filePath = settings.selectedStatisticsFilePath[0];
            return filePath.substring(0,filePath.lastIndexOf((filePath.indexOf("\\") > -1) ? "\\" : "/"));
        }


        //get renamed script file
        var GetScriptFileName = function() {
            var fileName = GetFileName();
            var fileNameNoExt = fileName.substring(0,fileName.indexOf("."));
            var scriptExt = settings.scriptFileName.substring(settings.scriptFileName.indexOf(".") + 1);
            return "{0}.{1}".format(fileNameNoExt,scriptExt);        
        }

        //map slash vs backslash
        var GetSlash = function() {
            var result = "/";
            if(os.platform() == "win32") { result = "\\"; }
            if(os.platform() == "darwin") { result = "/"; }
            return result;
        }

        //Update script file with new data table path & file name
        var UpdateScript = function() {
            var srcFilePath = GetFolderPath();
            srcFilePath += (srcFilePath.indexOf("\\") > -1) ? "\\{0}".format(GetScriptFileName()) : "/{0}".format(GetScriptFileName());
            fs.readFile(srcFilePath, (err, data) => {
                if (err) {
                    err.Handle(settings.outputStatisticsErrorSpn,settings.outputStatisticsErrorText,"Rigsarkiv.Hybris.DataExtraction.UpdateScript");
                }
                else {
                    var folderPath = GetFolderPath();
                    var filePath = folderPath;
                    filePath += (filePath.indexOf("\\") > -1) ? "\\{0}".format(GetScriptFileName()) : "/{0}".format(GetScriptFileName());
                    var fileName = GetFileName();  
                    var datafolderPath = settings.dataFolderPath;
                    var updatedData = data.toString().format(GetSlash(),folderPath,fileName.substring(0,fileName.indexOf(".")),datafolderPath);
                    console.logInfo(`Update script file ${filePath}`,"Rigsarkiv.Hybris.DataExtraction.UpdateScript");
                    fs.writeFile(filePath, updatedData, (err) => {
                        if (err) {
                            err.Handle(settings.outputStatisticsErrorSpn,settings.outputStatisticsErrorText,"Rigsarkiv.Hybris.DataExtraction.UpdateScript");
                        }
                        else {
                            var scriptFileName = GetScriptFileName();
                            settings.outputStatisticsOkCopyScriptSpn.innerHTML = Rigsarkiv.Language.callback().getValue("hybris-output-script-OKCopyScript").format(scriptFileName);
                            settings.outputStatisticsOkCopyScriptInfoSpn.innerHTML = Rigsarkiv.Language.callback().getValue("hybris-output-script-OKCopyScriptInfo").format(settings.scriptApplication);
                            settings.scriptPathLink.innerHTML = "[{0}]".format(GetFolderPath());
                            settings.scriptPanel1.hidden = true;
                            settings.scriptPanel2.hidden = false;
                            var folders = settings.dataFolderPath.getFolders();
                            settings.headerLinkTrin2.innerHTML = folders[folders.length - 1];
                            settings.headerLinkTrin3.innerHTML = folders[folders.length - 1];
                            settings.headerLinkInformation2.innerHTML = folders[folders.length - 1];
                        }
                    });
                }
            });
        }

        //copy related script file to data table folder 
        var CopyScript = function() { 
            var scriptFileName = GetScriptFileName();
            var scriptFilePath = settings.scriptPath.format(settings.scriptFileName);
            if(!fs.existsSync(scriptFilePath)) {
                var rootPath = null;
                if(os.platform() == "win32") {
                    rootPath = path.join('./');
                    scriptFilePath = path.join(rootPath,settings.resourceWinPath.format(settings.scriptFileName));
                }
                if(os.platform() == "darwin") {
                    var folders =  __dirname.split("/");
                    rootPath = folders.slice(0,folders.length - 4).join("/");
                    scriptFilePath = "{0}/{1}".format(rootPath,settings.scriptFileName);
                }
            }  
            var destFilePath = GetFolderPath();
            destFilePath += (destFilePath.indexOf("\\") > -1) ? "\\{0}".format(scriptFileName) : "/{0}".format(scriptFileName);
            console.logInfo(`copy script file ${settings.scriptFileName} to ${destFilePath}`,"Rigsarkiv.Hybris.DataExtraction.CopyScript");
            fs.copyFile(scriptFilePath, destFilePath, (err) => {
                if (err) {
                    err.Handle(settings.outputStatisticsErrorSpn,settings.outputStatisticsErrorText,"Rigsarkiv.Hybris.DataExtraction.CopyScript");
                }
                else {
                    var scriptFileName = GetScriptFileName();
                    var destFilePath = GetFolderPath();
                    console.logInfo(scriptFileName + ' was copied to ' + destFilePath,"Rigsarkiv.Hybris.DataExtraction.CopyScript");                
                    UpdateScript();                
                }            
            });       
        }   
        
        //copy script file related to selected Statistics file
        var EnsureScript = function() {
            var folderPath = GetFolderPath();
            fs.readdir(folderPath, (err, files) => {
                if (err) {
                    err.Handle(settings.outputStatisticsErrorSpn,settings.outputStatisticsErrorText,"Rigsarkiv.Hybris.DataExtraction.EnsureScript");
                }
                else {
                    var fileName = GetFileName();
                    var fileExt = fileName.substring(fileName.indexOf(".") + 1);
                    var sasCatalogFileName = settings.sasCatalogFileExt.format(fileName.substring(0,fileName.indexOf(".")));
                    var sasCatalogExists = false;
                    files.forEach(file => {
                        if(file === sasCatalogFileName) { sasCatalogExists = true; }
                    });
                    switch(fileExt) {
                        case "sav": {
                            settings.scriptApplication = "Statistikprogrammet SPSS Statistics";
                            settings.scriptType = "SPSS";
                            settings.scriptFileName = settings.scripts[0]; 
                        };break;
                        case "sas7bdat": {
                            settings.scriptApplication = "Statistikprogrammet SAS";
                            settings.scriptType = "SAS";
                            settings.scriptFileName = sasCatalogExists ? settings.scripts[2] : settings.scripts[1];
                        };break;
                        case "dta": { 
                            settings.scriptApplication = "Statistikprogrammet Stata";
                            settings.scriptType = "Stata";
                            settings.scriptFileName = settings.scripts[3];
                        }; break;
                    }
                    if(!sasCatalogExists && fileExt === "sas7bdat") {
                        ipcRenderer.send('open-confirm-dialog','dataextraction-catalogexists',Rigsarkiv.Language.callback().getValue("hybris-message-statistics-SASCatalogWarning-Title"),Rigsarkiv.Language.callback().getValue("hybris-message-statistics-SASCatalogWarning-Text"),Rigsarkiv.Language.callback().getValue("hybris-output-statistics-OkConfirm"),Rigsarkiv.Language.callback().getValue("hybris-output-statistics-CancelConfirm"));
                        settings.selectStatisticsFileBtn.disabled = true;
                        settings.okStatisticsBtn.disabled = true;
                        settings.outputStatisticsSASPopup.hidden = false;
                        return;
                    }
                    CopyScript();     
                }
            });
        }

        //delte current data table folder & reset inputs
        var ResetData = function() {
            settings.pathStatisticsFileTxt.value = "";
            fs.rmdir(settings.dataFolderPath, (err) => {
                if (err) {
                    err.Handle(settings.outputStatisticsErrorSpn,settings.outputStatisticsErrorText,"Rigsarkiv.Hybris.DataExtraction.ResetData");
                }
                else {
                    settings.dataFolderPath = null;
                    settings.scriptApplication = null;
                    settings.scriptType = null;
                    settings.scriptFileName = null; 
                    settings.selectedStatisticsFilePath = null;                
                    Reset();
                }
            });            
        }

        //create data tables folder structures
        var EnsureData = function() {
            settings.dataFolderPath = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath;
            settings.dataFolderPath += (settings.dataFolderPath.indexOf("\\") > -1) ? "\\{0}".format(settings.dataPathPostfix) : "/{0}".format(settings.dataPathPostfix);
            fs.readdir(settings.dataFolderPath, (err, files) => {
                if (err) {
                    settings.dataFolderPath = null;
                    err.Handle(settings.outputStatisticsErrorSpn,settings.outputStatisticsErrorText,"Rigsarkiv.Hybris.DataExtraction.EnsureData");
                }
                else {
                    var tablecounter = 0;
                    files.forEach(file => {
                        var counter = parseInt(file.substring(file.indexOf("table") + 5));
                        if(counter > tablecounter) {
                            tablecounter = counter;
                        }
                    });
                    tablecounter = tablecounter + 1;
                    var dataTableName = settings.dataTablePathPostfix.format(tablecounter);
                    settings.dataFolderPath += (settings.dataFolderPath.indexOf("\\") > -1) ? "\\{0}".format(dataTableName) : "/{0}".format(dataTableName);
                    fs.mkdir(settings.dataFolderPath, { recursive: true }, (err) => {
                        if (err) {
                            settings.dataFolderPath = null;
                            err.Handle(settings.outputStatisticsErrorSpn,settings.outputStatisticsErrorText,"Rigsarkiv.Hybris.DataExtraction.EnsureData");
                        }
                        else {
                            console.logInfo(`ensure package data path: ${settings.dataFolderPath}`,"Rigsarkiv.Hybris.DataExtraction.EnsureData");
                            EnsureScript();
                        }
                    });
                }            
            });
        }

        //validate file encoding
        var ValidateFile = function(fileName) {            
            var filePath = settings.dataFolderPath;
            filePath += (settings.dataFolderPath.indexOf("\\") > -1) ? "\\{0}".format(fileName) : "/{0}".format(fileName);
            var size = fs.statSync(filePath).size
            if(size <= MaxFileSize) {
                var charsetMatch = chardet.detectFileSync(filePath);
                console.logInfo("File {0} encode: {1}".format(fileName,charsetMatch),"Rigsarkiv.Hybris.DataExtraction.ValidateFile");
                if(charsetMatch !== "UTF-8") {
                    return false;
                }
            }
            return true;
        }

        //get Export files info  
        var GetExportInfo = function(files) {
            var unvalidFiles = [];
            var counter = 0;
            //var localPath = (settings.dataFolderPath.indexOf("\\") > -1) ? "{0}\\".format(settings.dataFolderPath) : "{0}/".format(settings.dataFolderPath);                           
            var fileName = GetFileName();
            var filePrefix = fileName.substring(0,fileName.indexOf("."));
            files.forEach(file => {
                settings.outputPostfixFiles.forEach(element => {
                    if(element.format(filePrefix) == file) 
                    { 
                        counter = counter + 1;
                        if(file.lastIndexOf("_VARIABEL.txt") > -1) { settings.variableFileName = file; }
                        //if(!ValidateFile(file)) { unvalidFiles.push(localPath + file); } 
                    }
                });
                /*settings.outputOptionalPostfixFiles.forEach(element => {
                    if(element.format(filePrefix) == file && !ValidateFile(file)) { unvalidFiles.push(localPath + file); }
                });*/
            });
            return { "counter":counter, "unvalidFiles":unvalidFiles};
        }

        //Ensure output files of statistic program
        var EnsureExport = function() {
            settings.outputScriptOkSpn.hidden = true;
            settings.spinner.className = settings.spinnerClass;
            fs.readdir(settings.dataFolderPath, (err, files) => {
                if (err) {
                    settings.spinner.className = "";
                    err.Handle(settings.outputStatisticsErrorSpn,settings.outputStatisticsErrorText,"Rigsarkiv.Hybris.DataExtraction.EnsureExport");
                }
                else {
                    settings.okScriptBtn.disabled = true;
                    var info = GetExportInfo(files);
                    var counter = info.counter;
                    var unvalidFiles = info.unvalidFiles;
                    settings.okScriptBtn.disabled = false;
                    settings.spinner.className = "";
                    if(counter < 3) {
                        ipcRenderer.send('open-warning-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-script-RequiredFilesWarning-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-script-RequiredFilesWarning-Text"));
                    }
                    else {
                        if(unvalidFiles.length > 0) 
                        { 
                            var filesText = unvalidFiles.join(",");
                            ipcRenderer.send('open-confirm-dialog','dataextraction-encodingfile',Rigsarkiv.Language.callback().getValue("hybris-output-script-EncodingFileError-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-script-EncodingFileError-Text").format(filesText),Rigsarkiv.Language.callback().getValue("hybris-output-statistics-OkConfirm"),Rigsarkiv.Language.callback().getValue("hybris-output-statistics-CancelConfirm")); 
                        }
                        else {
                            Redirect();
                        }
                    }
                }
            });
        }

        //remove UTF8 boom charactors
        var GetFileContent = function(filePath) {
            var fileContent = fs.readFileSync(filePath);
            var data = "";
            if(fileContent.byteLength >= 3 && (fileContent[0] & 0xff) == 0xef && (fileContent[1] & 0xff) == 0xbb && (fileContent[2] & 0xff) == 0xbf) {
                data = fileContent.toString("utf8",3);
            }
            else {
                data = fileContent.toString();
            }
            var splitChar = "\r";
            if(os.platform() == "win32") { splitChar = "\r\n"; }
            if(os.platform() == "darwin") { splitChar = "\n"; }
            return data.split(splitChar);
        }

         //create HTML select option element
         var CreateOption = function(value,text) {
            var result = document.createElement('option');
            result.textContent = text;
            result.value = value;
            return result;
        }

        //Add Backup information
        var AddBackup = function() {           
            fs.readdir(GetFolderPath(), (err, files) => {
                if (err) {
                    err.Handle(settings.outputStatisticsErrorSpn,settings.outputStatisticsErrorText,"Rigsarkiv.Hybris.DataExtraction.AddBackup");
                }
                else {
                    var totalSize = 0;
                    var path = GetFolderPath();
                    var folders = settings.dataFolderPath.getFolders();
                    var folderName =  folders[folders.length - 1];
                    var names = [];
                    files.forEach(fileName => {                        
                        var filePath = (path.indexOf("\\") > -1) ? "{0}\\{1}".format(path,fileName) : "{0}/{1}".format(path,fileName);
                        var fileState = fs.lstatSync(filePath);
                        if(fileState.isFile()) {
                            names.push(fileName);
                            totalSize += fileState.size;
                        }                        
                    });
                    Rigsarkiv.Hybris.Base.callback().backup.push({ "path":path, "name":folderName, "size":totalSize, "files":names });
                    settings.metdataTab.click();
                }
            }); 
        }

        //Redirect to metadata tab
        var Redirect = function() {
            var fileName = GetFileName();
            settings.outputScriptOkSpn.innerHTML = Rigsarkiv.Language.callback().getValue("hybris-output-script-Ok").format(fileName);
            settings.outputScriptOkSpn.hidden = false;
            settings.metadataFileName.value = fileName.substring(0,fileName.indexOf("."));
            console.logInfo("initialize variables Dropdown","Rigsarkiv.Hybris.DataExtraction.Redirect"); 
            try
            {
                $(settings.variablesDropdown).empty();
                settings.variablesDropdown.appendChild(CreateOption("",""));
                var destFilePath = settings.dataFolderPath;
                destFilePath += (destFilePath.indexOf("\\") > -1) ? "\\{0}".format(settings.variableFileName) : "/{0}".format(settings.variableFileName);
                var lines = GetFileContent(destFilePath);
                var i = 0;
                do {
                    var expressions = lines[i].trim().reduceWhiteSpace().split(" ");
                    if(expressions.length >= 2 && expressions[0] !== "") {
                        var variableName = expressions[0];
                        settings.variablesDropdown.appendChild(CreateOption(variableName,variableName));
                    }
                    i++;
                }
                while (lines[i] !== undefined && lines[i].trim() !== "");
                AddBackup();                
            }
            catch(err) 
            {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.DataExtraction.Redirect");
            }
            
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.headerLinkTrin2.addEventListener('click', (event) => {
                ipcRenderer.send('open-item',settings.dataFolderPath);
            })
            settings.headerLinkTrin3.addEventListener('click', (event) => {
                ipcRenderer.send('open-item',settings.dataFolderPath);
            })
            settings.headerLinkInformation2.addEventListener('click', (event) => {
                ipcRenderer.send('open-item',settings.dataFolderPath);
            })
            settings.okScriptDataPath.addEventListener('click', (event) => {
                ipcRenderer.send('open-item',GetFolderPath());
            })
            settings.scriptPathLink.addEventListener('click', (event) => {
                ipcRenderer.send('open-item', GetFolderPath());
            })
            settings.okScriptBtn.addEventListener('click', (event) => {
                EnsureExport();
            })
            settings.nextBtn.addEventListener('click', (event) => {
                settings.metdataTab.click();
            })
            settings.okStatisticsBtn.addEventListener('click', (event) => {
                Reset();
                if(settings.pathStatisticsFileTxt.value === "") {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-statistics-RequiredPath-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-statistics-RequiredPath-Text"));
                }
                if(settings.pathStatisticsFileTxt.value !== "" && Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath != null) {
                    EnsureData();
                }                     
            })
            settings.selectStatisticsFileBtn.addEventListener('click', (event) => {
                ipcRenderer.send('dataextraction-open-file-dialog');
            })
            ipcRenderer.on('dataextraction-selected-statistics-file', (event, path) => {
                settings.selectedStatisticsFilePath = path; 
                console.logInfo(`selected path: ${path}`,"Rigsarkiv.Hybris.DataExtraction.AddEvents"); 
                settings.pathStatisticsFileTxt.value = settings.selectedStatisticsFilePath;            
            })
            ipcRenderer.on('confirm-dialog-selection-dataextraction-catalogexists', (event, index) => {
                settings.selectStatisticsFileBtn.disabled = false;
                settings.okStatisticsBtn.disabled = false;
                settings.outputStatisticsSASPopup.hidden = true;
                if(index === 0) {
                    CopyScript();
                } 
                if(index === 1) { ResetData(); }            
            })
            ipcRenderer.on('confirm-dialog-selection-dataextraction-encodingfile', (event, index) => {
                if(index === 0) {
                    Redirect();
                } 
                if(index === 1) {  }            
            })
        }

        //Model interfaces functions
        Rigsarkiv.Hybris.DataExtraction = {        
            initialize: function (selectStatisticsFileId,pathStatisticsFileId,okStatisticsId,outputStatisticsErrorId,outputStatisticsOkCopyScriptId,scriptPanel1Id,scriptPanel2Id,okScriptBtnId,okScriptDataPathId,outputStatisticsOkCopyScriptInfoId,outputScriptOkId,nextId,metdataTabId,outputStructureOkId,selectStructureDeliveryPackageId,metadataFileName,spinnerId,outputScriptPath,outputHeaderLinkTrin2,outputHeaderLinkTrin3,outputHeaderLinkInformation2,outputStatisticsSASPopupId,variablesId) {
                settings.selectStatisticsFileBtn = document.getElementById(selectStatisticsFileId);
                settings.pathStatisticsFileTxt = document.getElementById(pathStatisticsFileId);
                settings.okStatisticsBtn = document.getElementById(okStatisticsId);
                settings.outputStatisticsErrorSpn = document.getElementById(outputStatisticsErrorId);
                settings.outputStatisticsErrorText = settings.outputStatisticsErrorSpn.innerHTML;
                settings.outputStatisticsOkCopyScriptSpn = document.getElementById(outputStatisticsOkCopyScriptId);
                settings.scriptPanel1 = document.getElementById(scriptPanel1Id);
                settings.scriptPanel2 = document.getElementById(scriptPanel2Id);
                settings.okScriptBtn = document.getElementById(okScriptBtnId);
                settings.okScriptDataPath = document.getElementById(okScriptDataPathId);
                settings.outputStatisticsOkCopyScriptInfoSpn = document.getElementById(outputStatisticsOkCopyScriptInfoId);
                settings.outputScriptOkSpn =  document.getElementById(outputScriptOkId);
                settings.nextBtn = document.getElementById(nextId);
                settings.metdataTab = document.getElementById(metdataTabId);
                settings.outputStructureOkSpn =  document.getElementById(outputStructureOkId);
                settings.selectStructureDeliveryPackage = document.getElementById(selectStructureDeliveryPackageId);
                settings.metadataFileName = document.getElementById(metadataFileName);
                settings.spinner = document.getElementById(spinnerId);
                settings.spinnerClass = settings.spinner.className;
                settings.spinner.className = "";
                settings.scriptPathLink = document.getElementById(outputScriptPath);
                settings.headerLinkTrin2 = document.getElementById(outputHeaderLinkTrin2);
                settings.headerLinkTrin3 = document.getElementById(outputHeaderLinkTrin3);
                settings.headerLinkInformation2 = document.getElementById(outputHeaderLinkInformation2);
                settings.outputStatisticsSASPopup = document.getElementById(outputStatisticsSASPopupId);
                settings.variablesDropdown = document.getElementById(variablesId);
                AddEvents();
            },
            callback: function () {
                return { 
                    dataFolderPath: settings.dataFolderPath, 
                    selectedStatisticsFilePath: settings.selectedStatisticsFilePath != null ? settings.selectedStatisticsFilePath[0] : null, 
                    scriptType: settings.scriptType, 
                    localFolderPath: settings.dataFolderPath, 
                    reset: function() 
                    { 
                        settings.pathStatisticsFileTxt.value = "";
                        Reset();
                    } 
                };
            }
        };
    }(jQuery);
}(jQuery);