window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {shell} = require('electron');
    const fs = require('fs');

    var settings = {
        outputErrorSpn: null,
        outputErrorText: null,
        filePath: null,
        selectedFilePath: null,
        logs: [],
        templateFileName: "log.html",
        scriptPath: "./assets/scripts/{0}",
        resourceWinPath: "resources/{0}",
        filePostfix: "{0}_log.html",
        errorElement: "<span id=\"{0}\"><i class=\"fas fa-times error\"></i>{1}</span><br/>",
        warnElement: "<span id=\"{0}\"><i class=\"fas fa-exclamation-triangle warning\"></i>{1}</span><br/>",
        infoElement: "<span id=\"{0}\"><i class=\"fas fa-check-circle ok\"></i>{1}</span><br/>" 
    }

    var HandleError = function(err) {
        console.log(`Error: ${err}`);
        settings.outputErrorSpn.hidden = false;
        settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);
    }

    var GetDateTime = function() {        
        var result = "";

        var today = new Date();
        result += today.getFullYear();        
        var temp = today.getMonth() + 1;
        result += (temp < 10) ? "0{0}".format(temp) : temp;
        var temp = today.getDate();
        result += (temp < 10) ? "0{0}".format(temp) : temp;
        var temp = today.getHours();
        result += (temp < 10) ? "0{0}".format(temp) : temp;
        var temp = today.getMinutes();
        result += (temp < 10) ? "0{0}".format(temp) : temp;
        var temp = today.getSeconds();
        result += (temp < 10) ? "0{0}".format(temp) : temp;
    
        return result;
    }
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
        fs.copyFile(logFilePath, settings.filePath, (err) => {
            if (err) {
                HandleError(err);
            }
            else {
                EnsureData();
            }
        });
    }

    var EnsureData = function() {
        fs.readFile(settings.filePath, (err, data) => {
            if (err) {
                HandleError(err);
            }
            else {
                var updatedData = data.toString().format(settings.logs.join("\r\n"));
                fs.writeFile(settings.filePath, updatedData, (err) => {
                    if (err) {
                        HandleError(err);
                    }
                    else {
                        settings.logs = [];
                        console.log("Log is updated at: {0}".format(settings.filePath));
                    }
                });
            }
        });
    }

    Rigsarkiv.Log = {
        initialize: function (outputErrorId) {
            settings.outputErrorSpn = document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML;
        },
        callback: function () {
            return { 
                error: function(text) 
                { 
                    console.log(`error ${text}`);
                    settings.logs.push(settings.errorElement.format(GetDateTime(),text));
                },
                warn: function(text) 
                { 
                    console.log(`warn ${text}`);
                    settings.logs.push(settings.warnElement.format(GetDateTime(),text));
                },
                info: function(text) 
                { 
                    console.log(`info ${text}`);
                    settings.logs.push(settings.infoElement.format(GetDateTime(),text));
                },
                commit: function(selectedFolderPath)
                {
                    settings.selectedFilePath = settings.filePostfix.format(selectedFolderPath);
                    settings.filePath = settings.filePostfix.format(selectedFolderPath.normlizePath());
                    if(!fs.existsSync(settings.filePath)) {
                        CopyFile();
                    }
                    else {
                        EnsureData();
                    }
                    return settings.selectedFilePath;
                } 
            };
        }
    }
}(jQuery);