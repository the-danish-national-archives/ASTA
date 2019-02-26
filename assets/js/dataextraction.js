window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron')
    const fs = require('fs');

    var settings = {
        structureCallback: null,
        selectStatisticsFileBtn: null,
        pathStatisticsFileTxt: null,
        okStatisticsBtn: null,
        outputStatisticsErrorSpn: null,
        outputStatisticsErrorText: null,
        outputStatisticsOkCopyScriptSpn: null,
        outputStatisticsOkCopyScriptText: null,
        selectedStatisticsFilePath: null,
        scriptPath: "./assets/scripts/{0}",
        scripts: ["spss_script.sps","sas_uden_katalog_script.sas","sas_med_katalog_script.sas","stata_script.do"]
    }

    var Reset = function () {
        settings.outputStatisticsErrorSpn.hidden = true;
        settings.outputStatisticsOkCopyScriptSpn.hidden = true;
    }

    var CopyScript = function() {
        var filePath = settings.selectedStatisticsFilePath[0];
        var folders = filePath.split("\\");
        var fileName = folders[folders.length - 1];
        var fileExt = fileName.substring(fileName.indexOf(".") + 1);
        var folderPath = filePath.substring(0,filePath.lastIndexOf("\\"));
        console.log(`path: ${folderPath}`);

        var scriptFileName = null;
        switch(fileExt) {
            case "sav": scriptFileName = settings.scripts[0]; break;
            case "sas7bdat": scriptFileName = settings.scripts[1]; break;
            case "dta": scriptFileName = settings.scripts[3]; break;
        }
        var scriptFilePath = settings.scriptPath.format(scriptFileName);
        console.log(`script file path: ${scriptFilePath}`);
        
        fs.copyFile(scriptFilePath, folderPath + "\\" + scriptFileName, (err) => {
            if (err) {
                settings.outputStatisticsErrorSpn.hidden = false;
                settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format(err.message);
            }
            else {
                console.log(scriptFileName + ' was copied to '+ folderPath);
                settings.outputStatisticsOkCopyScriptSpn.hidden = false;
                settings.outputStatisticsOkCopyScriptSpn.innerHTML = settings.outputStatisticsOkCopyScriptText.format(scriptFileName,folderPath);
            }            
          });
          
          //var structureFolderPath = settings.structureCallback();
        //console.log(`package path: ${structureFolderPath}`);
        
    }

    var AddEvents = function () {
        settings.okStatisticsBtn.addEventListener('click', (event) => {
            Reset();
            CopyScript();
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
        initialize: function (structureCallback,selectStatisticsFileId,pathStatisticsFileId,okStatisticsId,outputStatisticsErrorId,outputStatisticsOkCopyScriptId) {
            settings.structureCallback = structureCallback;
            settings.selectStatisticsFileBtn = document.getElementById(selectStatisticsFileId);
            settings.pathStatisticsFileTxt = document.getElementById(pathStatisticsFileId);
            settings.okStatisticsBtn = document.getElementById(okStatisticsId);
            settings.outputStatisticsErrorSpn = document.getElementById(outputStatisticsErrorId);
            settings.outputStatisticsErrorText = settings.outputStatisticsErrorSpn.innerHTML;
            settings.outputStatisticsOkCopyScriptSpn = document.getElementById(outputStatisticsOkCopyScriptId);
            settings.outputStatisticsOkCopyScriptText = settings.outputStatisticsOkCopyScriptSpn.innerHTML;
            AddEvents();
        }
    };
}(jQuery);