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
            filePostfix: "{0}_log.html",
            startLogData: "<div class=\"results\" id=\"testContent\">",
            endLogData: "</div>",
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
            console.log(`log path: ${filePath}`);
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
            folderName = folderName.substring(0,folderName.indexOf("_log.html"));
            var updatedData = data.toString().format(settings.logsDate.getFromFormat("dd-MM-yyyy hh:mm:ss"),settings.logs.join("\r\n"),settings.errorsCounter);
            fs.writeFileSync(settings.filePath, updatedData);
            console.log("Log is updated at: {0}".format(settings.filePath));
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
            console.log(`copy ${settings.templateFileName} file to: ${settings.filePath}`);
            fs.copyFileSync(logFilePath, settings.filePath);
        }
          
        //Validate each delivery Package
        var Validate = function() {
            var deliveryPackagePath = settings.deliveryPackages[settings.runIndex];
            console.log(`validate path: ${deliveryPackagePath}`); 
            var folders = deliveryPackagePath.getFolders();
            var logResult = settings.structureCallback().validate(deliveryPackagePath);
            settings.errorsCounter += logResult.errors;
            EnsureLogsData(logResult.filePath,folders[folders.length - 1]);
            settings.runIndex = settings.runIndex + 1;
            if(settings.runIndex < settings.deliveryPackages.length) {                
                setTimeout(Validate, 1000);
            } 
            else {
                settings.spinner.className = "";                
                CopyFile();
                EnsureData();                
            }           
        }

        //start run batching delete existing log
        var Run = function () {
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
            settings.filePath = settings.filePostfix.format(destPath);
            if(fs.existsSync(settings.filePath)) {                        
                console.log(`Delete exists log: ${settings.filePath}`);
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
                Run();
            })
            settings.selectDirBtn.addEventListener('click', (event) => {
                ipcRenderer.send('batch-open-file-dialog');
            })
            ipcRenderer.on('batch-selected-directory', (event, path) => {
                settings.selectedPath = path; 
                console.log(`selected path: ${path}`); 
                settings.pathDirTxt.value = settings.selectedPath;
            })
            settings.selectLogfile.addEventListener('click', (event) => {
                shell.openItem(settings.filePath);
            });            
        }

        //Model interfaces functions
        Rigsarkiv.Nemesis.Batch = { 
            initialize: function (rightsCallback,structureCallback,outputErrorId,selectDirectoryId,pathDirectoryId,validateId,spinnerId,okId,selectLogfileId,supplementId,panelId) {            
                settings.rightsCallback = rightsCallback;
                settings.structureCallback = structureCallback;
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.selectDirBtn = document.getElementById(selectDirectoryId);
                settings.pathDirTxt = document.getElementById(pathDirectoryId);
                settings.validateBtn = document.getElementById(validateId);
                settings.spinner = document.getElementById(spinnerId);
                settings.spinnerClass = settings.spinner.className;
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