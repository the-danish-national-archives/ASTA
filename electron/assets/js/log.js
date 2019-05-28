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
        outputOkText: null,
        selectLogfile: null,
        outputSupplementSpn: null,
        filePath: null,
        logs: [],
        logsDate: null,
        errorsCounter: 0,
        templateFileName: "log.html",
        scriptPath: "./assets/scripts/{0}",
        resourceWinPath: "resources\\{0}",
        filePostfix: "{0}_ASTA_log.html",
        errorElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"error\">{3}</span>",
        warnElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"warning\" hidden=\"true\">{3}</span>",
        infoElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"ok\" hidden=\"true\">{3}</span>",
        sectionElement: "<span id=\"{0}_{1}\" name=\"{2}\" class=\"section\">{3}</span>",
        spinner: null,
        spinnerClass: null
    }

    //reset status & input fields
    var Reset = function () {
        settings.outputErrorSpn.hidden = true;
        settings.outputOkSpn.hidden = true;
        settings.selectLogfile.hidden = true;
        settings.outputSupplementSpn.hidden = true;
    }

    //commit log data
    var EnsureData = function() {
        var data = fs.readFileSync(settings.filePath);        
        var folders = settings.filePath.getFolders();
        var folderName = folders[folders.length - 1];
        folderName = folderName.substring(0,folderName.indexOf("_ASTA_log.html"));
        var updatedData = data.toString().format(settings.logsDate.getFromFormat("dd-MM-yyyy hh:mm:ss"),folderName,settings.logs.join("\r\n"),settings.errorsCounter);
        fs.writeFileSync(settings.filePath, updatedData);                         
        settings.logs = [];
        settings.errorsCounter = 0;
        settings.logsDate = null;                        
        console.log("Log is updated at: {0}".format(settings.filePath));
        var folders = settings.filePath.getFolders();
        var folderName = folders[folders.length - 1];
        settings.selectLogfile.innerHTML = settings.filePath;
        settings.outputOkSpn.innerHTML = settings.outputOkText.format(folderName);
        settings.selectLogfile.hidden = false;
        settings.outputOkSpn.hidden = false;
        settings.outputSupplementSpn.hidden = false;                        
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
        EnsureData();        
    }

    //add Event Listener to HTML elmenets
    var AddEvents = function () {
        settings.selectLogfile.addEventListener('click', (event) => {
            shell.openItem(settings.filePath);
        }); 
    }

    //Model interfaces functions
    Rigsarkiv.Log = {
        initialize: function (outputErrorId,outputOkId,selectLogfileId,outputSupplementId, spinnerId) {
            settings.outputErrorSpn = document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML;
            settings.outputOkSpn =  document.getElementById(outputOkId);
            settings.outputOkText = settings.outputOkSpn.innerHTML;
            settings.selectLogfile = document.getElementById(selectLogfileId);
            settings.outputSupplementSpn =  document.getElementById(outputSupplementId);
            settings.spinner = document.getElementById(spinnerId);
            settings.spinnerClass = settings.spinner.className;
            settings.spinner.className = "";
            AddEvents();               
        },
        callback: function () {
            return { 
                error: function(logType,folderName,text) 
                { 
                    if(settings.logsDate == null) { settings.logsDate = new Date(); }
                    console.log(`error ${text}`);
                    settings.logs.push(settings.errorElement.format(logType,(new Date()).getFromFormat("yyyyMMddhhmmss"),folderName,text));
                    settings.errorsCounter += 1;
                    // start spinner
                    settings.spinner.className = settings.spinnerClass;
                },
                warn: function(logType,folderName,text) 
                { 
                    if(settings.logsDate == null) { settings.logsDate = new Date(); }
                    console.log(`warn ${text}`);
                    settings.logs.push(settings.warnElement.format(logType,(new Date()).getFromFormat("yyyyMMddhhmmss"),folderName,text));
                    // start spinner
                    settings.spinner.className = settings.spinnerClass;
                },
                info: function(logType,folderName,text) 
                { 
                    if(settings.logsDate == null) { settings.logsDate = new Date(); }
                    console.log(`info ${text}`);
                    settings.logs.push(settings.infoElement.format(logType,(new Date()).getFromFormat("yyyyMMddhhmmss"),folderName,text));
                    // start spinner
                    settings.spinner.className = settings.spinnerClass;
                },
                section: function(logType,folderName,text) 
                { 
                    if(settings.logsDate == null) { settings.logsDate = new Date(); }
                    console.log(`info ${text}`);
                    settings.logs.push(settings.sectionElement.format(logType,(new Date()).getFromFormat("yyyyMMddhhmmss"),folderName,text));
                    // start spinner
                    settings.spinner.className = settings.spinnerClass;
                },
                commit: function(selectedFolderPath)
                {
                    Reset(); 
                    try
                    {
                        settings.filePath = settings.filePostfix.format(selectedFolderPath);
                        if(fs.existsSync(settings.filePath)) {                        
                            console.log(`Delete exists log: ${settings.filePath}`);
                            fs.unlinkSync(settings.filePath);
                        }
                        var errorsCounter = settings.errorsCounter;
                        CopyFile();
                        // stop spinner
                        settings.spinner.className = ""; 
                        return { 
                            filePath: settings.filePath,
                            errors: errorsCounter
                        };
                    }
                    catch(err) 
                    {
                        err.Handle(settings.outputErrorSpn,settings.outputErrorText);
                    }
                } 
            };
        }
    }
}(jQuery);