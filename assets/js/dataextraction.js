window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron')
    const fs = require('fs');

    var settings = {
        structureCallback: null,
        scriptFileName: null,
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
        scriptPath: "./assets/scripts/{0}",
        scripts: ["spss_script.sps","sas_uden_katalog_script.sas","sas_med_katalog_script.sas","stata_script.do"],
        sasCatalogFileExt: "{0}.sas7bcat"
    }

    var Reset = function () {
        settings.outputStatisticsErrorSpn.hidden = true;
        settings.outputStatisticsOkCopyScriptSpn.hidden = true;
    }

    var GetFolderPath = function() {
        var filePath = settings.selectedStatisticsFilePath[0];
        return filePath.substring(0,filePath.lastIndexOf("\\"));
    }

    var CopyScript = function() {
        var scriptFilePath = settings.scriptPath.format(settings.scriptFileName);
        console.log(`script file path: ${scriptFilePath}`);

        var folderPath = GetFolderPath();
        fs.copyFile(scriptFilePath, folderPath + "\\" + settings.scriptFileName, (err) => {
            if (err) {
                settings.outputStatisticsErrorSpn.hidden = false;
                settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format(err.message);
            }
            else {
                console.log(settings.scriptFileName + ' was copied to '+ folderPath);
                settings.outputStatisticsOkCopyScriptSpn.hidden = false;
                settings.outputStatisticsOkCopyScriptSpn.innerHTML = settings.outputStatisticsOkCopyScriptText.format(settings.scriptFileName,folderPath);
            }            
        });
          
          //var structureFolderPath = settings.structureCallback();
        //console.log(`package path: ${structureFolderPath}`);
    }   
    
    var EnsureScript = function() {
        
        fs.readdir(GetFolderPath(), (err, files) => {
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

    var AddEvents = function () {
        settings.okStatisticsBtn.addEventListener('click', (event) => {
            Reset();
            EnsureScript();
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
        initialize: function (structureCallback,selectStatisticsFileId,pathStatisticsFileId,okStatisticsId,outputStatisticsErrorId,outputStatisticsOkCopyScriptId,outputStatisticsSASWarningPrefixId) {
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
            AddEvents();
        }
    };
}(jQuery);