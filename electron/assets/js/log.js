/*
    Model is responsible for Logging to HTML file
    initialize interface inputs: 
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {shell} = require('electron');
    const fs = require('fs');
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
        logs: [],
        logsDate: null,
        errorsCounter: 0,
        templateFileName: "log.html",
        scriptPath: "./assets/scripts/{0}",
        resourceWinPath: "resources\\{0}",
        filePostfix: "{0}_ASTA_testlog.html",
        errorElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"error\">{3}</span>",
        warnElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"warning\">{3}</span>",
        infoElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"ok\" hidden=\"true\">{3}</span>",
        sectionElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"section\">{3}</span>",
        spinner: null,
        spinnerClass: null,
        spinnerEnable: true,
        outputShowBtn: null
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
        var data = fs.readFileSync(settings.filePath);        
        var folders = settings.filePath.getFolders();
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
        var updatedData = data.toString().format(settings.logsDate.getFromFormat("dd-MM-yyyy hh:mm:ss"),folderName,settings.logs.join("\r\n"),testEnd,title,runDate,filters,error,warning,testTitle,testStart);
        fs.writeFileSync(settings.filePath, updatedData);                         
        settings.logs = [];
        settings.errorsCounter = 0;
        settings.logsDate = null;                        
        console.logInfo("Log is updated at: {0}".format(settings.filePath),"Rigsarkiv.Log.EnsureData");
        var folders = settings.filePath.getFolders();
        var folderName = folders[folders.length - 1];
        settings.selectLogfile.innerHTML = settings.filePath;
        settings.outputOkSpn.innerHTML = Rigsarkiv.Language.callback().getValue("nemesis-output-Ok").format(folderName);
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
        console.logInfo(`copy ${settings.templateFileName} file to: ${settings.filePath}`,"Rigsarkiv.Log.CopyFile");
        fs.copyFileSync(logFilePath, settings.filePath);
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
            shell.openItem(settings.filePath);
        }); 
        settings.outputShowBtn.addEventListener('click', (event) => {
            shell.openItem(settings.filePath);
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
                commit: function(selectedFolderPath)
                {
                    Reset(); 
                    try
                    {
                        settings.filePath = settings.filePostfix.format(selectedFolderPath);
                        if(fs.existsSync(settings.filePath)) {                        
                            console.logInfo(`Delete exists log: ${settings.filePath}`,"Rigsarkiv.Log.callback.commit");
                            fs.unlinkSync(settings.filePath);
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