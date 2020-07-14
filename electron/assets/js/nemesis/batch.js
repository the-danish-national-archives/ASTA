/*
    Model is responsible for Batch validation
    initialize interface inputs: elements from <div id="nemesis-batch-test-panel">
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Nemesis = Rigsarkiv.Nemesis || {},
    function (n) {
        const {ipcRenderer} = require('electron');
        const {shell} = require('electron');
        const electron = require('electron');
        const fs = require('fs'); 
        const fse = require('fs-extra');
        const path = require('path');
        const os = require('os');   

        //private data memebers
        var settings = { 
            outputErrorSpn: null,
            outputErrorText: null,
            selectDirBtn: null,
            pathDirTxt: null,
            selectedPath: null,
            workingPath: null,
            validateBtn: null,
            validatePanel: null,
            spinner: null,
            spinnerClass: null,
            rightsCallback: null,
            structureCallback: null,
            okSpn: null,
            selectLogfile: null,
            supplementSpn: null,
            filePath: null,
            fileName: null,
            deliveryPackages: [],
            logs: [],
            logsDate: null,
            runIndex: 0,
            templateFileName: "batchlog.html",
            scriptPath: "./assets/scripts/{0}",
            resourceWinPath: "resources\\{0}",
            filePostfix: "{0}_ASTA_testlog.html",
            pathPostfix: "{0}ASTA_testlog_{1}",
            batchFilePostfix: "{0}_ASTA_BatchLog.html",
            batchPathPostfix: "{0}ASTA_Batchlog_{1}",
            prefixLogFolder: "ASTA_",
            startLogData: "<!--Start Log Data-->",
            endLogData: "<!--End Log Data-->",
            errorsCounter: 0
        }
        
        //get log file full path
        var GetDestPath = function () {
            return (settings.filePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.filePath,settings.fileName) : "{0}/{1}".format(settings.filePath, settings.fileName);
        }
        
        //reset status & input fields
        var Reset = function () {
            settings.okSpn.hidden = true;
            settings.selectLogfile.hidden = true;
            settings.supplementSpn.hidden = true;            
        }

        // collect logs data
        var EnsureLogsData = function(filePath,folderName) {
            console.logInfo(`log path: ${filePath}`,"Rigsarkiv.Nemesis.Batch.EnsureLogsData");
            var data = fs.readFileSync(filePath).toString();
            var startIndex = data.indexOf(settings.startLogData);
            data = data.substr(startIndex + settings.startLogData.length);
            var endIndex = data.indexOf(settings.endLogData);
            data = data.substr(0,endIndex);
            settings.logs.push(data);
        }

        //commit log data
        var EnsureData = function() {
            var destPath = GetDestPath();
            var data = fs.readFileSync(destPath);        
            var folders = destPath.getFolders();
            var folderName = folders[folders.length - 1];
            folderName = folderName.substring(0,folderName.indexOf(".html"));
            var title = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-Title");
            var titleHeader = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-Title-H1");
            var runDate = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-runDate-P");
            var filters = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-filters-SPAN");
            var error = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-error-LABEL");
            var warning = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-warning-LABEL");
            var selectorLabel = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-Selector-LABEL");
            var selectorOption = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-Selector-OPTION");
            var testTitle = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-test-Title-H3");
            var testStart = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-test-Start-P");
            var testEnd = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-test-End-P").format(settings.errorsCounter);
            var wholeContains = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-test-WholeContains");
            var contains = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-test-Contains");
            var errors = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-test-Errors");
            var versionText = Rigsarkiv.Language.callback().getValue("nemesis-batchFile-test-version-P")
            var versionNo = versionText + electron.remote["app"].getVersion();
            var updatedData = data.toString().format(settings.logsDate.getFromFormat("dd-MM-yyyy hh:mm:ss"),settings.logs.join("\r\n"),testEnd,folderName,title,titleHeader,runDate,filters,error,warning,selectorLabel,selectorOption,testTitle,testStart,wholeContains,contains,errors,versionNo);
            console.logInfo('filepath: ' + destPath + ' selected path:' + settings.selectedPath[0],"Rigsarkiv.Nemesis.Batch.EnsureData");
            fs.writeFileSync(destPath, updatedData);
            console.logInfo("Log is updated at: {0}".format(destPath),"Rigsarkiv.Nemesis.Batch.EnsureData");
            settings.logs = [];
            settings.errorsCounter = 0;
            settings.logsDate = null;
            var folderName = folders[folders.length - 1];
            settings.selectLogfile.innerHTML = destPath;
            settings.okSpn.innerHTML = Rigsarkiv.Language.callback().getValue("nemesis-batch-Ok").format(folderName);
            settings.selectLogfile.hidden = false;
            settings.okSpn.hidden = false;
            settings.supplementSpn.hidden = false;                        
    }


         //copy log HTML template file to parent folder of selected Delivery Package folder
        var CopyFile = function() {
            var logFilePath = settings.scriptPath.format(settings.templateFileName);        
            if(!fs.existsSync(logFilePath)) {
                var rootPath = null;
                if(os.platform() == "win32") {
                    rootPath = path.join('./');
                    logFilePath = path.join(rootPath,settings.resourceWinPath.format(settings.templateFileName));
                }
                if(os.platform() == "darwin") {
                    var folders =  __dirname.split("/");
                    rootPath = folders.slice(0,folders.length - 3).join("/");
                    logFilePath = "{0}/{1}".format(rootPath,settings.templateFileName);
                }
            } 
            fs.mkdirSync(settings.filePath); 
            var destPath = GetDestPath();      
            console.logInfo(`copy ${settings.templateFileName} file to: ${destPath}`,"Rigsarkiv.Nemesis.Batch.CopyFile");
            fs.copyFileSync(logFilePath, destPath);
        }
        
        //Ensure existing of Log File
        var EnsureLogFile = function() {
            var deliveryPackagePath = settings.deliveryPackages[settings.runIndex];
            var folders = deliveryPackagePath.getFolders();
            var folderName = folders[folders.length - 1];
            var logFileName = settings.filePostfix.format(folderName);            
            var logFilePath = settings.pathPostfix.format(settings.workingPath,folderName + (settings.workingPath.indexOf("\\") > -1 ? "\\" : "/") + logFileName);
            console.logInfo(`Ensure log path: ${logFilePath}`,"Rigsarkiv.Nemesis.Batch.EnsureLogFile");
            if(fs.existsSync(logFilePath)) { 
                EnsureLogsData(logFilePath,folderName);
                settings.runIndex = settings.runIndex + 1;
                if(settings.runIndex < settings.deliveryPackages.length) {                
                    setTimeout(Validate, 500);
                } 
                else {
                    settings.spinner.className = "";                
                    CopyFile();
                    EnsureData(); 
                    settings.selectDirBtn.disabled = false;
                    settings.validateBtn.disabled = false;               
                }           
            }
            else {
                setTimeout(EnsureLogFile, 500);
            }
        }

        //Validate each delivery Package
        var Validate = function() {
            var deliveryPackagePath = settings.deliveryPackages[settings.runIndex];
            var folders = deliveryPackagePath.getFolders();
            var logFilePath = settings.pathPostfix.format(settings.workingPath,folders[folders.length - 1]);
            if(fs.existsSync(logFilePath)) {                        
                console.logInfo(`Delete exists log: ${logFilePath}`,"Rigsarkiv.Nemesis.Batch.Validate");
                fse.removeSync(logFilePath);
            }
            console.logInfo(`validate path: ${deliveryPackagePath}`,"Rigsarkiv.Nemesis.Batch.Validate");             
            settings.structureCallback().validate(deliveryPackagePath);
            EnsureLogFile();            
        }

        //start run batching delete existing log
        var Run = function () {
            settings.selectDirBtn.disabled = true;
            settings.validateBtn.disabled = true;
            settings.logsDate = new Date();            
            settings.spinner.className = settings.spinnerClass;
            settings.workingPath = settings.selectedPath[0];
            var folders = settings.workingPath.getFolders();
            fs.readdirSync(settings.workingPath).forEach(folder => {
                var deliveryPackagePath = (settings.workingPath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.workingPath,folder) : "{0}/{1}".format(settings.workingPath,folder);
                if(fs.statSync(deliveryPackagePath).isDirectory() && !folder.startsWith(settings.prefixLogFolder)) {
                    settings.deliveryPackages.push(deliveryPackagePath);
                }
            });
            settings.workingPath += settings.workingPath.indexOf("\\") > -1 ? "\\" : "/"; 
            var folderName = folders[folders.length - 1];    
            settings.fileName = settings.batchFilePostfix.format(folderName);
            settings.filePath = settings.batchPathPostfix.format(settings.workingPath,folderName);                        
            if(fs.existsSync(settings.filePath)) {                        
                console.logInfo(`Delete exists log: ${settings.filePath}`,"Rigsarkiv.Nemesis.Batch.Run");
                fse.removeSync(settings.filePath);
            }
            if(settings.deliveryPackages.length > 0) {
                setTimeout(Validate, 1000);
            }            
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.validateBtn.addEventListener('click', (event) => {
                if(settings.selectedPath == null || settings.pathDirTxt.value === "") { return; }
                try {
                    Run();
                }
                catch(err) 
                {
                    err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Nemesis.Batch.AddEvents");
                } 
            })
            settings.selectDirBtn.addEventListener('click', (event) => {
                ipcRenderer.send('batch-open-file-dialog');
            })
            ipcRenderer.on('batch-selected-directory', (event, path) => {
                settings.selectedPath = path; 
                console.logInfo(`selected path: ${path}`,"Rigsarkiv.Nemesis.Batch.AddEvents"); 
                settings.pathDirTxt.value = settings.selectedPath;
                Reset();
            })
            settings.selectLogfile.addEventListener('click', (event) => {
                shell.openItem(GetDestPath());
            });            
        }

        //Model interfaces functions
        Rigsarkiv.Nemesis.Batch = { 
            initialize: function (rightsCallback,structureCallback,outputErrorId,selectDirectoryId,pathDirectoryId,validateId,spinnerId,okId,selectLogfileId,supplementId,panelId,spinnerClassName) {            
                settings.rightsCallback = rightsCallback;
                settings.structureCallback = structureCallback;
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.selectDirBtn = document.getElementById(selectDirectoryId);
                settings.pathDirTxt = document.getElementById(pathDirectoryId);
                settings.validateBtn = document.getElementById(validateId);
                settings.spinner = document.getElementById(spinnerId);
                settings.spinnerClass = spinnerClassName;
                settings.spinner.className = "";
                settings.okSpn =  document.getElementById(okId);
                settings.selectLogfile = document.getElementById(selectLogfileId);
                settings.supplementSpn =  document.getElementById(supplementId);
                settings.panel =  document.getElementById(panelId);
                settings.panel.hidden = !settings.rightsCallback().isAdmin;              
                AddEvents();
            }
        };

    }(jQuery);
}(jQuery);