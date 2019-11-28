/*
    Model is responsible for Logging to HTML file
    initialize interface inputs: 
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {shell} = require('electron');
    const fs = require('fs');
    const fse = require('fs-extra');
    const path = require('path');
    const os = require('os');

    //private data memebers
    var settings = {
        outputErrorSpn: null,
        outputErrorText: null,
        outputOkSpn: null,
        selectLogfile: null,
        outputSupplementSpn: null,
        filePath: null,
        fileName: null,
        logs: [],
        logsDate: null,
        errorsCounter: 0,
        templateFileName: "log.html",
        scriptPath: "./assets/scripts/{0}",
        resourceWinPath: "resources\\{0}",
        filePostfix: "{0}_ASTA_testlog.html",
        pathPostfix: "{0}ASTA_testlog_{1}",
        errorElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"error\">{3}</span>",
        warnElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"warning\">{3}</span>",
        infoElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"ok\" hidden=\"true\">{3}</span>",
        sectionElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"section\">{3}</span>",
        spinner: null,
        spinnerClass: null,
        spinnerEnable: true,
        outputShowBtn: null,
        confirmation: null
    }

    //get log file full path
    var GetDestPath = function () {
        return (settings.filePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.filePath,settings.fileName) : "{0}/{1}".format(settings.filePath, settings.fileName);
    }

    //reset status & input fields
    var Reset = function () {
        settings.outputErrorSpn.hidden = true;
        settings.outputOkSpn.hidden = true;
        settings.selectLogfile.hidden = true;
        settings.outputSupplementSpn.hidden = true;
        settings.outputShowBtn.hidden = true;
    }

    //commit log data
    var EnsureData = function() {
        var destPath = GetDestPath();
        var data = fs.readFileSync(destPath);        
        var folders = destPath.getFolders();
        var folderName = folders[folders.length - 1];
        folderName = folderName.substring(0,folderName.indexOf("_ASTA_testlog.html"));
        var title = Rigsarkiv.Language.callback().getValue("nemesis-logFile-Title");
        var runDate = Rigsarkiv.Language.callback().getValue("nemesis-logFile-runDate-P");
        var filters = Rigsarkiv.Language.callback().getValue("nemesis-logFile-filters-SPAN");
        var error = Rigsarkiv.Language.callback().getValue("nemesis-logFile-error-LABEL");
        var warning = Rigsarkiv.Language.callback().getValue("nemesis-logFile-warning-LABEL");
        var testTitle = Rigsarkiv.Language.callback().getValue("nemesis-logFile-test-Title-H3");
        var testStart = Rigsarkiv.Language.callback().getValue("nemesis-logFile-test-Start-P");
        var testEnd = Rigsarkiv.Language.callback().getValue("nemesis-logFile-test-End-P").format(settings.errorsCounter);
        var updatedData = data.toString().format(settings.logsDate.getFromFormat("dd-MM-yyyy hh:mm:ss"),folderName,settings.logs.join("\r\n"),testEnd,title,runDate,filters,error,warning,testTitle,testStart,settings.confirmation);
        fs.writeFileSync(destPath, updatedData);                         
        settings.logs = [];
        settings.errorsCounter = 0;
        settings.logsDate = null;                        
        console.logInfo("Log is updated at: {0}".format(destPath),"Rigsarkiv.Log.EnsureData");
        settings.selectLogfile.innerHTML = destPath;
        settings.outputOkSpn.innerHTML = Rigsarkiv.Language.callback().getValue("nemesis-output-Ok").format(folders[folders.length - 1]);
        settings.selectLogfile.hidden = false;
        settings.outputOkSpn.hidden = false;
        settings.outputSupplementSpn.hidden = false; 
        settings.outputShowBtn.hidden = false;                       
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
        console.logInfo(`copy ${settings.templateFileName} file to: ${destPath}`,"Rigsarkiv.Log.CopyFile");
        fs.copyFileSync(logFilePath, destPath);
        EnsureData();        
    }

    //strib log information id's
    var LogInfo = function(text,stack) {
        if(stack !== "Rigsarkiv.Log.callback.section") {
            var startIndex = text.indexOf("<div id=\"");
            if(startIndex > -1) { 
                startIndex = startIndex + 9;               
                console.logInfo(text.substring(startIndex,text.indexOf("\"",startIndex)),stack);
            }
        }
        else {
            console.logInfo(text,stack);   
        }
                 
    }

    //add Event Listener to HTML elmenets
    var AddEvents = function () {
        settings.selectLogfile.addEventListener('click', (event) => {
            shell.openItem(GetDestPath());
        }); 
        settings.outputShowBtn.addEventListener('click', (event) => {
            shell.openItem(GetDestPath());
        }); 
    }

    //Model interfaces functions
    Rigsarkiv.Log = {
        initialize: function (outputErrorId,outputOkId,selectLogfileId,outputSupplementId, spinnerId,spinnerClassName,outputShowId) {
            settings.outputErrorSpn = document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML;
            settings.outputOkSpn =  document.getElementById(outputOkId);
            settings.selectLogfile = document.getElementById(selectLogfileId);
            settings.outputSupplementSpn =  document.getElementById(outputSupplementId);
            settings.spinner = document.getElementById(spinnerId);
            settings.spinnerClass = spinnerClassName;
            settings.spinner.className = "";
            settings.outputShowBtn = document.getElementById(outputShowId);
            AddEvents();               
        },
        callback: function () {
            return {
                reset: function() 
                { 
                    Reset();
                }, 
                spinnerEnable: function(enable)
                {
                    settings.spinnerEnable = enable;
                },
                error: function(logType,folderName,text) 
                { 
                    if(settings.logsDate == null) { settings.logsDate = new Date(); }
                    LogInfo(`error ${text}`,"Rigsarkiv.Log.callback.error");
                    settings.logs.push(settings.errorElement.format(logType,(new Date()).getFromFormat("yyyyMMddhhmmss"),folderName,text));
                    settings.errorsCounter += 1;
                    if(settings.spinnerEnable && settings.spinner.className === "") { settings.spinner.className = settings.spinnerClass; }
                },
                warn: function(logType,folderName,text) 
                { 
                    if(settings.logsDate == null) { settings.logsDate = new Date(); }
                    LogInfo(`warn ${text}`,"Rigsarkiv.Log.callback.warn");
                    settings.logs.push(settings.warnElement.format(logType,(new Date()).getFromFormat("yyyyMMddhhmmss"),folderName,text));
                    if(settings.spinnerEnable && settings.spinner.className === "") { settings.spinner.className = settings.spinnerClass; }
                },
                info: function(logType,folderName,text) 
                { 
                    if(settings.logsDate == null) { settings.logsDate = new Date(); }
                    LogInfo(`info ${text}`,"Rigsarkiv.Log.callback.info");
                    settings.logs.push(settings.infoElement.format(logType,(new Date()).getFromFormat("yyyyMMddhhmmss"),folderName,text));
                    if(settings.spinnerEnable && settings.spinner.className === "") { settings.spinner.className = settings.spinnerClass; }
                },
                section: function(logType,folderName,text) 
                { 
                    if(settings.logsDate == null) { settings.logsDate = new Date(); }
                    LogInfo(`section ${text}`,"Rigsarkiv.Log.callback.section");
                    settings.logs.push(settings.sectionElement.format(logType,(new Date()).getFromFormat("yyyyMMddhhmmss"),folderName,text));
                    if(settings.spinnerEnable && settings.spinner.className === "") { settings.spinner.className = settings.spinnerClass; }
                },
                commit: function(selectedFolderPath,confirmation)
                {
                    Reset(); 
                    try
                    {
                        var folders = selectedFolderPath.getFolders();
                        var fileName = folders[folders.length - 1];
                        settings.fileName = settings.filePostfix.format(fileName);
                        settings.confirmation = confirmation;
                        settings.filePath = settings.pathPostfix.format(selectedFolderPath.substring(0,selectedFolderPath.lastIndexOf((selectedFolderPath.indexOf("\\") > -1) ? "\\" : "/") + 1),fileName);
                        if(fs.existsSync(settings.filePath)) {                        
                            console.logInfo(`Delete exists log: ${settings.filePath}`,"Rigsarkiv.Log.callback.commit");
                            fse.removeSync(settings.filePath);
                        }
                        var errorsCounter = settings.errorsCounter;
                        CopyFile();
                        if(settings.spinnerEnable) { settings.spinner.className = ""; }
                        return { 
                            filePath: settings.filePath,
                            errors: errorsCounter
                        };
                    }
                    catch(err) 
                    {
                        err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Log.callback.commit");
                    }
                } 
            };
        }
    }
}(jQuery);