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
        const fs = require('fs'); 
        const path = require('path');
        const os = require('os');   

        //private data memebers
        var settings = { 
            outputErrorSpn: null,
            outputErrorText: null,
            selectDirBtn: null,
            pathDirTxt: null,
            selectedPath: null,
            validateBtn: null,
            validatePanel: null,
            spinner: null,
            spinnerClass: null,
            rightsCallback: null,
            structureCallback: null,
            okSpn: null,
            okText: null,
            selectLogfile: null,
            supplementSpn: null,
            filePath: null,
            deliveryPackages: [],
            logs: [],
            logsDate: null,
            runIndex: 0,
            templateFileName: "batchlog.html",
            scriptPath: "./assets/scripts/{0}",
            resourceWinPath: "resources\\{0}",
            filePostfix: "{0}_ASTA_log.html",
            batchFilePostfix: "{0}_ASTA_BatchLog.html",
            startLogData: "<!--Start Log Data-->",
            endLogData: "<!--End Log Data-->",
            errorsCounter: 0
        } 
        
        //reset status & input fields
        var Reset = function () {
            settings.okSpn.hidden = true;
            settings.selectLogfile.hidden = true;
            settings.supplementSpn.hidden = true;            
        }

        // collect logs data
        var EnsureLogsData = function(filePath,folderName) {
            console.log(`log path: ${filePath}`,"Rigsarkiv.Nemesis.Batch.EnsureLogsData");
            var data = fs.readFileSync(filePath).toString();
            var startIndex = data.indexOf(settings.startLogData);
            data = data.substr(startIndex + settings.startLogData.length);
            var endIndex = data.indexOf(settings.endLogData);
            data = data.substr(0,endIndex);
            settings.logs.push(data);
        }

        //commit log data
        var EnsureData = function() {
            var data = fs.readFileSync(settings.filePath);        
            var folders = settings.filePath.getFolders();
            var folderName = folders[folders.length - 1];
            folderName = folderName.substring(0,folderName.indexOf(".html"));
            var updatedData = data.toString().format(settings.logsDate.getFromFormat("dd-MM-yyyy hh:mm:ss"),settings.logs.join("\r\n"),settings.errorsCounter, folderName);
            console.log('filepath: ' + settings.filePath + ' selected path:' + settings.selectedPath[0],"Rigsarkiv.Nemesis.Batch.EnsureData");
            fs.writeFileSync(settings.filePath, updatedData);
            console.log("Log is updated at: {0}".format(settings.filePath),"Rigsarkiv.Nemesis.Batch.EnsureData");
            settings.logs = [];
            settings.errorsCounter = 0;
            settings.logsDate = null;
            var folders = settings.filePath.getFolders();
            var folderName = folders[folders.length - 1];
            settings.selectLogfile.innerHTML = settings.filePath;
            settings.okSpn.innerHTML = settings.okText.format(folderName);
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
            console.log(`copy ${settings.templateFileName} file to: ${settings.filePath}`,"Rigsarkiv.Nemesis.Batch.CopyFile");
            fs.copyFileSync(logFilePath, settings.filePath);
        }
        
        //Ensure existing of Log File
        var EnsureLogFile = function() {
            var deliveryPackagePath = settings.deliveryPackages[settings.runIndex]; 
            var logFilePath = settings.filePostfix.format(deliveryPackagePath);
            console.log(`Ensure log path: ${logFilePath}`,"Rigsarkiv.Nemesis.Batch.EnsureLogFile");
            if(fs.existsSync(logFilePath)) { 
                var folders = deliveryPackagePath.getFolders();
                EnsureLogsData(logFilePath,folders[folders.length - 1]);
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
            var logFilePath = settings.filePostfix.format(deliveryPackagePath);
            if(fs.existsSync(logFilePath)) {                        
                console.log(`Delete exists log: ${logFilePath}`,"Rigsarkiv.Nemesis.Batch.Validate");
                fs.unlinkSync(logFilePath);
            }
            console.log(`validate path: ${deliveryPackagePath}`,"Rigsarkiv.Nemesis.Batch.Validate");             
            settings.structureCallback().validate(deliveryPackagePath);
            EnsureLogFile();            
        }

        //start run batching delete existing log
        var Run = function () {
            settings.selectDirBtn.disabled = true;
            settings.validateBtn.disabled = true;
            settings.logsDate = new Date();            
            settings.spinner.className = settings.spinnerClass;
            Reset();
            var destPath = settings.selectedPath[0];
            fs.readdirSync(destPath).forEach(folder => {
                var deliveryPackagePath = (destPath.indexOf("\\") > -1) ? "{0}\\{1}".format(destPath,folder) : "{0}/{1}".format(destPath,folder);
                if(fs.statSync(deliveryPackagePath).isDirectory()) {
                    settings.deliveryPackages.push(deliveryPackagePath);
                }
            });
            var folders = destPath.getFolders();
            var batchFileName = settings.batchFilePostfix.format(folders[folders.length - 1]);
            settings.filePath = (destPath.indexOf("\\") > -1) ? "{0}\\{1}".format(destPath, batchFileName) : "{0}/{1}".format(destPath, batchFileName);
            if(fs.existsSync(settings.filePath)) {                        
                console.log(`Delete exists log: ${settings.filePath}`,"Rigsarkiv.Nemesis.Batch.Run");
                fs.unlinkSync(settings.filePath);
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
                console.log(`selected path: ${path}`,"Rigsarkiv.Nemesis.Batch.AddEvents"); 
                settings.pathDirTxt.value = settings.selectedPath;
            })
            settings.selectLogfile.addEventListener('click', (event) => {
                shell.openItem(settings.filePath);
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
                settings.okText = settings.okSpn.innerHTML;
                settings.selectLogfile = document.getElementById(selectLogfileId);
                settings.supplementSpn =  document.getElementById(supplementId);
                settings.panel =  document.getElementById(panelId);
                settings.panel.hidden = !settings.rightsCallback().isAdmin;              
                AddEvents();
            }
        };

    }(jQuery);
}(jQuery);