window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron')
    const fs = require('fs');

    var settings = {
        structureCallback: null,
        scriptFileName: null,
        dataFolderPath: null,
        selectStatisticsFileBtn: null,
        pathStatisticsFileTxt: null,
        okStatisticsBtn: null,
        outputStatisticsErrorSpn: null,
        outputStatisticsErrorText: null,
        outputStatisticsOkCopyScriptSpn: null,
        outputStatisticsOkCopyScriptText: null,
        outputStatisticsSASWarningTitle: null,
        outputStatisticsSASWarningText: null,
        selectedStatisticsFilePath: null,
        scriptPanel: null,  
        okScriptBtn: null,      
        scriptPath: "./assets/scripts/{0}",
        scripts: ["spss_script.sps","sas_uden_katalog_script.sas","sas_med_katalog_script.sas","stata_script.do"],
        sasCatalogFileExt: "{0}.sas7bcat",
        dataPathPostfix: "\\Data",
        dataTablePathPostfix: "\\table{0}"
    }

    var Reset = function () {
        settings.outputStatisticsErrorSpn.hidden = true;
        settings.outputStatisticsOkCopyScriptSpn.hidden = true;
        settings.scriptPanel.hidden = true;
    }

    var CopyScript = function() {
        var scriptFilePath = settings.scriptPath.format(settings.scriptFileName);
        console.log(`script file path: ${scriptFilePath}`);

        fs.copyFile(scriptFilePath, settings.dataFolderPath + "\\" + settings.scriptFileName, (err) => {
            if (err) {
                settings.outputStatisticsErrorSpn.hidden = false;
                settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format(err.message);
            }
            else {
                console.log(settings.scriptFileName + ' was copied to '+ settings.dataFolderPath);
                settings.outputStatisticsOkCopyScriptSpn.hidden = false;
                settings.outputStatisticsOkCopyScriptSpn.innerHTML = settings.outputStatisticsOkCopyScriptText.format(settings.scriptFileName,settings.dataFolderPath);
                settings.scriptPanel.hidden = false;
            }            
        });
    }   
    
    var EnsureScript = function() {
        var filePath = settings.selectedStatisticsFilePath[0];
        var folderPath = filePath.substring(0,filePath.lastIndexOf("\\"));
        fs.readdir(folderPath, (err, files) => {
            if (err) {
                settings.outputStatisticsErrorSpn.hidden = false;
                settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format(err.message);
            }
            else {
                var folders = settings.selectedStatisticsFilePath[0].split("\\");
                var fileName = folders[folders.length - 1];
                var fileExt = fileName.substring(fileName.indexOf(".") + 1);
                var sasCatalogFileName = settings.sasCatalogFileExt.format(fileName.substring(0,fileName.indexOf(".")));
                var sasCatalogExists = false;
                files.forEach(file => {
                    if(file === sasCatalogFileName) {
                        sasCatalogExists = true;
                    }
                });

                switch(fileExt) {
                    case "sav": settings.scriptFileName = settings.scripts[0]; break;
                    case "sas7bdat": {
                        settings.scriptFileName = sasCatalogExists ? settings.scripts[2] : settings.scripts[1];
                    };break;
                    case "dta": settings.scriptFileName = settings.scripts[3]; break;
                }
                if(!sasCatalogExists && fileExt === "sas7bdat") {
                    ipcRenderer.send('open-warning-dialog',settings.outputStatisticsSASWarningTitle.innerHTML,settings.outputStatisticsSASWarningText.innerHTML);
                }
                CopyScript();     
            }
        });
    }

    var EnsureData = function() {
        var structureFolderPath = settings.structureCallback();
        if(structureFolderPath == null) { return; }
        settings.dataFolderPath =  structureFolderPath + settings.dataPathPostfix;
                
        fs.readdir(settings.dataFolderPath, (err, files) => {
            if (err) {
                settings.dataFolderPath = null;
                settings.outputStatisticsErrorSpn.hidden = false;
                settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format(err.message);
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
                        settings.outputStatisticsErrorSpn.hidden = false;
                        settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format(err.message);
                    }
                    else {
                        console.log(`ensure package data path: ${settings.dataFolderPath}`);
                        EnsureScript();
                    }
                });
            }            
        });
    }

    var AddEvents = function () {
        settings.okScriptBtn.addEventListener('click', (event) => {
            
        })
        settings.okStatisticsBtn.addEventListener('click', (event) => {
            Reset();
            EnsureData();            
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
        initialize: function (structureCallback,selectStatisticsFileId,pathStatisticsFileId,okStatisticsId,outputStatisticsErrorId,outputStatisticsOkCopyScriptId,outputStatisticsSASWarningPrefixId,scriptPanelId,okScriptBtnId) {
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
            AddEvents();
        }
    };
}(jQuery);