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
        const {shell} = require('electron');
        const fs = require('fs');
        const path = require('path');
        const chardet = require('chardet');
        const os = require('os');

        //private data memebers
        var settings = {
            structureCallback: null,
            scriptType: null,
            scriptApplication: null,
            scriptFileName: null,
            dataFolderPath: null,
            selectStatisticsFileBtn: null,
            pathStatisticsFileTxt: null,
            okStatisticsBtn: null,
            outputStatisticsErrorSpn: null,
            outputStatisticsErrorText: null,
            outputStatisticsOkCopyScriptSpn: null,
            outputStatisticsOkCopyScriptText: null,
            outputStatisticsOkCopyScriptInfoSpn: null,
            outputStatisticsOkCopyScriptInfoText: null,        
            outputStatisticsSASWarningTitle: null,
            outputStatisticsSASWarningText: null,
            outputScriptRequiredFilesWarningTitle: null,
            outputScriptRequiredFilesWarningText: null,
            outputStatisticsRequiredPathTitle: null,
            outputStatisticsRequiredPathText: null,
            outputScriptEncodingFileErrorTitle: null,
            outputScriptEncodingFileErrorText: null,
            outputScriptCloseApplicationWarningTitle: null,
            outputScriptCloseApplicationWarningText: null,
            selectedStatisticsFilePath: null,
            outputScriptOkSpn: null,
            outputScriptOkText: null,
            scriptPanel1: null,
            scriptPanel2: null,  
            okScriptBtn: null,
            nextBtn: null,  
            metdataTab: null, 
            okScriptDataPath: null,   
            scriptPath: "./assets/scripts/{0}",
            resourceWinPath: "resources\\{0}",
            scripts: ["spss_script.sps","sas_uden_katalog_script.sas","sas_med_katalog_script.sas","stata_script.do"],
            outputPostfixFiles: ["{0}.csv","{0}_VARIABEL.txt","{0}_VARIABELBESKRIVELSE.txt"],
            outputOptionalPostfixFiles: ["{0}_KODELISTE.txt","{0}_BRUGERKODE.txt"],
            sasCatalogFileExt: "{0}.sas7bcat",
            dataPathPostfix: "Data",
            dataTablePathPostfix: "table{0}"
        }

        //output system error messages
        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputStatisticsErrorSpn.hidden = false;
            settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format(err.message);
        }

        //reset status & input fields
        var Reset = function () {        
            settings.outputStatisticsErrorSpn.hidden = true;
            settings.scriptPanel1.hidden = false;
            settings.scriptPanel2.hidden = true;
            settings.outputScriptOkSpn.hidden = true;
            settings.nextBtn.hidden = true;
        }

        //get selected Statistics File name
        var GetFileName = function() {
            var filePath = settings.selectedStatisticsFilePath[0];
            var folders = filePath.getFolders();
            return folders[folders.length - 1];
        }

        //get renamed script file
        var GetScriptFileName = function() {
            var fileName = GetFileName();
            var fileNameNoExt = fileName.substring(0,fileName.indexOf("."));
            var scriptExt = settings.scriptFileName.substring(settings.scriptFileName.indexOf(".") + 1);
            return "{0}.{1}".format(fileNameNoExt,scriptExt);        
        }

        //Copy Statistics file to data table folder
        var CopyData = function(fileName) {
            var filePath = settings.selectedStatisticsFilePath[0];
            var folderPath = filePath.substring(0,filePath.lastIndexOf((filePath.indexOf("\\") > -1) ? "\\" : "/"));
            
            console.log("copy file " + fileName + " to  " + folderPath);  
            var srcFilePath = (folderPath.indexOf("\\") > -1) ? "{0}\\{1}".format(folderPath,fileName) : "{0}/{1}".format(folderPath,fileName);   
            var destFilePath = (settings.dataFolderPath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.dataFolderPath,fileName) : "{0}/{1}".format(settings.dataFolderPath,fileName);
            fs.copyFile(srcFilePath , destFilePath, (err) => {
                if (err) {
                    HandleError(err);
                }
            });
        }

        //Update script file with new dtat table path & file name
        var UpdateScript = function() {
            var srcFilePath = settings.dataFolderPath;
            srcFilePath += (srcFilePath.indexOf("\\") > -1) ? "\\{0}".format(GetScriptFileName()) : "/{0}".format(GetScriptFileName());
            fs.readFile(srcFilePath, (err, data) => {
                if (err) {
                    HandleError(err);
                }
                else {
                     var filePath = settings.dataFolderPath;
                    filePath += (filePath.indexOf("\\") > -1) ? "\\{0}".format(GetScriptFileName()) : "/{0}".format(GetScriptFileName());
                    var fileName = GetFileName();  
                    var folderPath = settings.dataFolderPath;
                    if(fileName.substring(fileName.indexOf(".") + 1) === "sas7bdat") 
                    { 
                        folderPath = (folderPath.indexOf("\\") > -1) ? "{0}\\".format(folderPath) : "{0}/".format(folderPath);
                    }
                    var updatedData = data.toString().format(folderPath,fileName.substring(0,fileName.indexOf(".")));
                    fs.writeFile(filePath, updatedData, (err) => {
                        if (err) {
                            HandleError(err);
                        }
                        else {
                            var scriptFileName = GetScriptFileName();
                            settings.outputStatisticsOkCopyScriptSpn.innerHTML = settings.outputStatisticsOkCopyScriptText.format(settings.scriptType,scriptFileName,GetFileName());
                            settings.outputStatisticsOkCopyScriptInfoSpn.innerHTML = settings.outputStatisticsOkCopyScriptInfoText.format(scriptFileName, settings.scriptApplication);
                            settings.okScriptDataPath.innerHTML = settings.dataFolderPath;
                            settings.scriptPanel1.hidden = true;
                            settings.scriptPanel2.hidden = false;
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
                    rootPath = folders.slice(0,folders.length - 3).join("/");
                    scriptFilePath = "{0}/{1}".format(rootPath,settings.scriptFileName);
                }
            }        
            console.log(`copy script file ${settings.scriptFileName} to ${settings.dataFolderPath}`);
            var destFilePath = settings.dataFolderPath;
            destFilePath += (destFilePath.indexOf("\\") > -1) ? "\\{0}".format(scriptFileName) : "/{0}".format(scriptFileName);
            fs.copyFile(scriptFilePath, destFilePath, (err) => {
                if (err) {
                HandleError(err);
                }
                else {
                    var scriptFileName = GetScriptFileName();
                    console.log(scriptFileName + ' was copied to '+ settings.dataFolderPath);                
                    UpdateScript();                
                }            
            });       
        }   
        
        //copy script file related to selected Statistics file
        var EnsureScript = function() {
            var filePath = settings.selectedStatisticsFilePath[0];
            var folderPath = filePath.substring(0,filePath.lastIndexOf((filePath.indexOf("\\") > -1) ? "\\" : "/"));
            fs.readdir(folderPath, (err, files) => {
                if (err) {
                    HandleError(err);
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
                        ipcRenderer.send('open-confirm-dialog',settings.outputStatisticsSASWarningTitle.innerHTML,settings.outputStatisticsSASWarningText.innerHTML);
                        return;
                    }
                    if(sasCatalogExists && fileExt === "sas7bdat") {  CopyData(sasCatalogFileName); }
                    CopyData(fileName);
                    CopyScript();     
                }
            });
        }

        //delte current data table folder & reset inputs
        var ResetData = function() {
            settings.pathStatisticsFileTxt.value = "";
            fs.rmdir(settings.dataFolderPath, (err) => {
                if (err) {
                    HandleError(err);
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
            settings.dataFolderPath = settings.structureCallback().deliveryPackagePath;
            settings.dataFolderPath += (settings.dataFolderPath.indexOf("\\") > -1) ? "\\{0}".format(settings.dataPathPostfix) : "/{0}".format(settings.dataPathPostfix);
            fs.readdir(settings.dataFolderPath, (err, files) => {
                if (err) {
                    settings.dataFolderPath = null;
                    HandleError(err);
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
                            HandleError(err);
                        }
                        else {
                            console.log(`ensure package data path: ${settings.dataFolderPath}`);
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
            var charsetMatch = chardet.detectFileSync(filePath);
            console.log("File {0} encode: {1}".format(fileName,charsetMatch));
            if(charsetMatch !== "UTF-8") {
                var localPath = settings.dataFolderPath;
                localPath += (localPath.indexOf("\\") > -1) ? "\\{0}".format(fileName) : "/{0}".format(fileName);
                ipcRenderer.send('open-error-dialog',settings.outputScriptEncodingFileErrorTitle.innerHTML,settings.outputScriptEncodingFileErrorText.innerHTML.format(localPath));
                return false;
            }
            return true;
        }

        //Ensure output files of statistic program
        var EnsureExport = function() {
            settings.outputScriptOkSpn.hidden = true;
            fs.readdir(settings.dataFolderPath, (err, files) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    var fileValidate = true;
                    var counter = 0;
                    var fileName = GetFileName();
                    var filePrefix = fileName.substring(0,fileName.indexOf("."));
                    files.forEach(file => {
                        settings.outputPostfixFiles.forEach(element => {
                            if(element.format(filePrefix) == file) 
                            { 
                                counter = counter + 1;
                                if(!ValidateFile(file)) { fileValidate = false; } 
                            }
                        });
                        settings.outputOptionalPostfixFiles.forEach(element => {
                            if(element.format(filePrefix) == file && !ValidateFile(file)) { fileValidate = false; }
                        });
                    });
                    if(counter < 3) {
                        ipcRenderer.send('open-warning-dialog',settings.outputScriptRequiredFilesWarningTitle.innerHTML,settings.outputScriptRequiredFilesWarningText.innerHTML);
                    }
                    else {
                        if(fileValidate) 
                        { 
                            ipcRenderer.send('open-warning-dialog',settings.outputScriptCloseApplicationWarningTitle.innerHTML,settings.outputScriptCloseApplicationWarningText.innerHTML);
                            settings.outputScriptOkSpn.innerHTML = settings.outputScriptOkText.format(GetFileName());
                            settings.outputScriptOkSpn.hidden = false;
                            settings.nextBtn.hidden = false;
                        }
                    }
                }
            });
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.okScriptDataPath.addEventListener('click', (event) => {
                shell.openItem(settings.dataFolderPath);
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
                    ipcRenderer.send('open-error-dialog',settings.outputStatisticsRequiredPathTitle.innerHTML,settings.outputStatisticsRequiredPathText.innerHTML);
                }
                if(settings.pathStatisticsFileTxt.value !== "" && settings.structureCallback != null && settings.structureCallback().deliveryPackagePath != null) {
                    EnsureData();
                }                     
            })
            settings.selectStatisticsFileBtn.addEventListener('click', (event) => {
            ipcRenderer.send('dataextraction-open-file-dialog');
            })
            ipcRenderer.on('dataextraction-selected-statistics-file', (event, path) => {
                settings.selectedStatisticsFilePath = path; 
                console.log(`selected path: ${path}`); 
                settings.pathStatisticsFileTxt.value = settings.selectedStatisticsFilePath;            
            })
            ipcRenderer.on('confirm-dialog-selection', (event, index) => {
                if(index === 0) {
                    CopyData(GetFileName());
                    CopyScript();
                } 
                if(index === 1) { ResetData(); }            
            })
        }

        //Model interfaces functions
        Rigsarkiv.Hybris.DataExtraction = {        
            initialize: function (structureCallback,selectStatisticsFileId,pathStatisticsFileId,okStatisticsId,outputStatisticsErrorId,outputStatisticsOkCopyScriptId,outputStatisticsSASWarningPrefixId,scriptPanel1Id,scriptPanel2Id,okScriptBtnId,okScriptDataPathId,outputStatisticsOkCopyScriptInfoId,outputStatisticsRequiredPathId,outputScriptRequiredFilesWarningPrefixId,outputScriptOkId,outputScriptEncodingFileErrorPrefixId,nextId,metdataTabId,outputScriptCloseApplicationWarningPrefixId) {
                settings.structureCallback = structureCallback;
                settings.selectStatisticsFileBtn = document.getElementById(selectStatisticsFileId);
                settings.pathStatisticsFileTxt = document.getElementById(pathStatisticsFileId);
                settings.okStatisticsBtn = document.getElementById(okStatisticsId);
                settings.outputStatisticsErrorSpn = document.getElementById(outputStatisticsErrorId);
                settings.outputStatisticsErrorText = settings.outputStatisticsErrorSpn.innerHTML;
                settings.outputStatisticsOkCopyScriptSpn = document.getElementById(outputStatisticsOkCopyScriptId);
                settings.outputStatisticsOkCopyScriptText = settings.outputStatisticsOkCopyScriptSpn.innerHTML;
                settings.outputStatisticsSASWarningTitle = document.getElementById(outputStatisticsSASWarningPrefixId + "-Title");
                settings.outputStatisticsSASWarningText = document.getElementById(outputStatisticsSASWarningPrefixId + "-Text");
                settings.scriptPanel1 = document.getElementById(scriptPanel1Id);
                settings.scriptPanel2 = document.getElementById(scriptPanel2Id);
                settings.okScriptBtn = document.getElementById(okScriptBtnId);
                settings.okScriptDataPath = document.getElementById(okScriptDataPathId);
                settings.outputStatisticsOkCopyScriptInfoSpn = document.getElementById(outputStatisticsOkCopyScriptInfoId);
                settings.outputStatisticsOkCopyScriptInfoText = settings.outputStatisticsOkCopyScriptInfoSpn.innerHTML;
                settings.outputStatisticsRequiredPathTitle = document.getElementById(outputStatisticsRequiredPathId + "-Title");
                settings.outputStatisticsRequiredPathText = document.getElementById(outputStatisticsRequiredPathId + "-Text");
                settings.outputScriptRequiredFilesWarningTitle = document.getElementById(outputScriptRequiredFilesWarningPrefixId + "-Title");
                settings.outputScriptRequiredFilesWarningText = document.getElementById(outputScriptRequiredFilesWarningPrefixId + "-Text");
                settings.outputScriptOkSpn =  document.getElementById(outputScriptOkId);
                settings.outputScriptOkText = settings.outputScriptOkSpn.innerHTML;
                settings.outputScriptEncodingFileErrorTitle = document.getElementById(outputScriptEncodingFileErrorPrefixId + "-Title");
                settings.outputScriptEncodingFileErrorText = document.getElementById(outputScriptEncodingFileErrorPrefixId + "-Text");
                settings.nextBtn = document.getElementById(nextId);
                settings.metdataTab = document.getElementById(metdataTabId);
                settings.outputScriptCloseApplicationWarningTitle = document.getElementById(outputScriptCloseApplicationWarningPrefixId + "-Title");
                settings.outputScriptCloseApplicationWarningText = document.getElementById(outputScriptCloseApplicationWarningPrefixId + "-Text");
                AddEvents();
            },
            callback: function () {
                return { 
                    structureCallback: settings.structureCallback(),
                    dataFolderPath: settings.dataFolderPath, 
                    selectedStatisticsFilePath: settings.selectedStatisticsFilePath[0], 
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