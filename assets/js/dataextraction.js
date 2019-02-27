window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron')
    const {shell} = require('electron')
    const fs = require('fs');

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
        outputStatisticsRequiredPathSpn: null,
        selectedStatisticsFilePath: null,
        scriptPanel: null,  
        okScriptBtn: null,   
        okScriptDataPath: null,   
        scriptPath: "./assets/scripts/{0}",
        scripts: ["spss_script.sps","sas_uden_katalog_script.sas","sas_med_katalog_script.sas","stata_script.do"],
        outputPostfixFiles: ["{0}.csv","{0}_VARIABEL.txt","{0}_VARIABELBESKRIVELSE.txt"],
        sasCatalogFileExt: "{0}.sas7bcat",
        dataPathPostfix: "/Data",
        dataTablePathPostfix: "/table{0}"
    }

    var HandleError = function(err) {
        console.log(`Error: ${err}`);
        settings.outputStatisticsErrorSpn.hidden = false;
        settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format(err.message);
    }

    var Reset = function () {
        settings.outputStatisticsRequiredPathSpn.hidden = true;
        settings.outputStatisticsErrorSpn.hidden = true;
        settings.scriptPanel.hidden = true;
        settings.outputScriptOkSpn.hidden = true;
    }

    var GetLocalFolderPath = function() {
        var folderPath = settings.structureCallback().selectedPath;
        if(folderPath.indexOf("\\") > -1) {
            var folders = settings.dataFolderPath.split("/");
            return "{0}\\{1}\\{2}\\{3}".format(folderPath,folders[folders.length - 3],folders[folders.length - 2],folders[folders.length - 1]);
        }
        else {
            return settings.dataFolderPath;
        }
    } 

    var GetFileName = function() {
        var filePath = settings.selectedStatisticsFilePath[0].normlizePath();
        var folders = filePath.split("/");
        return folders[folders.length - 1];
    }

    var CopyData = function(fileName) {
        var filePath = settings.selectedStatisticsFilePath[0].normlizePath();
        console.log(`copy data file to: ${filePath}`);        
        fs.copyFile(filePath, settings.dataFolderPath + "/" + fileName, (err) => {
            if (err) {
                HandleError(err);
            }
        });
    }

    var CopyScript = function() {        
        var scriptFilePath = settings.scriptPath.format(settings.scriptFileName);
        console.log(`copy script file to: ${scriptFilePath}`);
        fs.copyFile(scriptFilePath, settings.dataFolderPath + "/" + settings.scriptFileName, (err) => {
            if (err) {
                HandleError(err);
            }
            else {
                console.log(settings.scriptFileName + ' was copied to '+ settings.dataFolderPath);
                var filePath = settings.dataFolderPath + "/" + settings.scriptFileName;
                fs.readFile(filePath, (err, data) => {
                    if (err) {
                        HandleError(err);
                    }
                    else {
                        var filePath = settings.dataFolderPath + "/" + settings.scriptFileName;
                        var fileName = GetFileName();
                        fileName = fileName.substring(0,fileName.indexOf("."));
                        var updatedData = data.toString().format(settings.dataFolderPath,fileName);
                        fs.writeFile(filePath, updatedData, (err) => {
                            if (err) {
                                HandleError(err);
                            }
                            else {
                                settings.outputStatisticsOkCopyScriptSpn.innerHTML = settings.outputStatisticsOkCopyScriptText.format(settings.scriptType,settings.scriptFileName,GetFileName());
                                settings.outputStatisticsOkCopyScriptInfoSpn.innerHTML = settings.outputStatisticsOkCopyScriptInfoText.format(settings.scriptFileName, settings.scriptApplication);
                                settings.okScriptDataPath.innerHTML = GetLocalFolderPath();
                                settings.scriptPanel.hidden = false;
                            }
                        });
                    }
                  });                
            }            
        });       
    }   
    
    var EnsureScript = function() {
        var filePath = settings.selectedStatisticsFilePath[0].normlizePath();
        var folderPath = filePath.substring(0,filePath.lastIndexOf("/"));
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
                    ipcRenderer.send('open-warning-dialog',settings.outputStatisticsSASWarningTitle.innerHTML,settings.outputStatisticsSASWarningText.innerHTML);
                }
                if(sasCatalogExists && fileExt === "sas7bdat") {  CopyData(sasCatalogFileName); }
                CopyData(fileName);
                CopyScript();     
            }
        });
    }

    var EnsureData = function() {
        var structureFolderPath = settings.structureCallback().deliveryPackagePath;
        if(structureFolderPath == null) { return; }
        settings.dataFolderPath =  structureFolderPath + settings.dataPathPostfix;
                
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
                settings.dataFolderPath += settings.dataTablePathPostfix.format(tablecounter);
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

    var EnsureExport = function() {
        settings.outputScriptOkSpn.hidden = true;
        fs.readdir(settings.dataFolderPath, (err, files) => {
            if (err) {
                HandleError(err);
            }
            else {
                var counter = 0;
                var fileName = GetFileName();
                var filePrefix = fileName.substring(0,fileName.indexOf("."))
                files.forEach(file => {
                    settings.outputPostfixFiles.forEach(element => {
                        if(element.format(filePrefix) == file) { counter = counter + 1; }
                    });
                });
                if(counter < 3) {
                    ipcRenderer.send('open-warning-dialog',settings.outputScriptRequiredFilesWarningTitle.innerHTML,settings.outputScriptRequiredFilesWarningText.innerHTML);
                }
                else {
                    settings.outputScriptOkSpn.hidden = false;
                }
            }
        });
    }

    var AddEvents = function () {
        settings.okScriptDataPath.addEventListener('click', (event) => {
            shell.openItem(GetLocalFolderPath());
        })
        settings.okScriptBtn.addEventListener('click', (event) => {
            EnsureExport();
        })
        settings.okStatisticsBtn.addEventListener('click', (event) => {
            Reset();
            if(settings.pathStatisticsFileTxt.value === "") {
                settings.outputStatisticsRequiredPathSpn.hidden = false;
            }
            if(settings.pathStatisticsFileTxt.value !== "" && settings.structureCallback != null && settings.structureCallback().deliveryPackagePath != null) {
                EnsureData();
            }
            else {
                settings.outputStatisticsErrorSpn.hidden = false;
                settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format("No delivery Package Path");
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
    }

    Rigsarkiv.DataExtraction = {        
        initialize: function (structureCallback,selectStatisticsFileId,pathStatisticsFileId,okStatisticsId,outputStatisticsErrorId,outputStatisticsOkCopyScriptId,outputStatisticsSASWarningPrefixId,scriptPanelId,okScriptBtnId,okScriptDataPathId,outputStatisticsOkCopyScriptInfoId,outputStatisticsRequiredPathId,outputScriptRequiredFilesWarningPrefixId,outputScriptOkId) {
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
            settings.scriptPanel = document.getElementById(scriptPanelId);
            settings.okScriptBtn = document.getElementById(okScriptBtnId);
            settings.okScriptDataPath = document.getElementById(okScriptDataPathId);
            settings.outputStatisticsOkCopyScriptInfoSpn = document.getElementById(outputStatisticsOkCopyScriptInfoId);
            settings.outputStatisticsOkCopyScriptInfoText = settings.outputStatisticsOkCopyScriptInfoSpn.innerHTML;
            settings.outputStatisticsRequiredPathSpn = document.getElementById(outputStatisticsRequiredPathId);
            settings.outputScriptRequiredFilesWarningTitle = document.getElementById(outputScriptRequiredFilesWarningPrefixId + "-Title");
            settings.outputScriptRequiredFilesWarningText = document.getElementById(outputScriptRequiredFilesWarningPrefixId + "-Text");
            settings.outputScriptOkSpn =  document.getElementById(outputScriptOkId);
            AddEvents();
        },
        callback: function () {
            return { dataFolderPath: settings.dataFolderPath, selectedStatisticsFilePath: settings.selectedStatisticsFilePath[0] };
        }
    };
}(jQuery);