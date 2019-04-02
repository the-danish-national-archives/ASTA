/*
    Model is responsible for Validate Metadata tables (Flow 2.0)
    initialize interface inputs: elements from <div class="formularContainer">
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Nemesis = Rigsarkiv.Nemesis || {},
    function (n) {
        const {ipcRenderer} = require('electron');
        const fs = require('fs');
        const chardet = require('chardet');

        //private data memebers
        var settings = { 
            outputErrorSpn: null,
            outputErrorText: null,
            outputPrefix: null,
            logCallback: null,
            logStartSpn: null,
            logEndNoErrorSpn: null,
            logEndWithErrorSpn:null,
            deliveryPackagePath: null,
            outputText: {},
            logType: "metadata",
            errorsCounter: 0,
            errorStop: false,
            dataPathPostfix: "Data",
            metadataLabels: ["SYSTEMNAVN","DATAFILNAVN","DATAFILBESKRIVELSE","NØGLEVARIABEL","REFERENCE","VARIABEL","VARIABELBESKRIVELSE","KODELISTE","BRUGERKODE"]
        }

        //output system error messages
        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputErrorSpn.hidden = false;
            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);
        }

        // get selected folder name 
        var GetFolderName = function() {
            var folders = settings.deliveryPackagePath.getFolders();
            return folders[folders.length - 1];
        }
        // View Element by id & return texts
        var ViewElement = function(id,formatText1,formatText2,formatText3) {
            var result = settings.outputText[id];
            if(formatText1 != null) { 
                if(formatText2 != null) {
                    if(formatText3 != null) {
                        result = result.format(formatText1,formatText2,formatText3);
                    }
                    else {
                        result = result.format(formatText1,formatText2);
                    }
                }
                else {
                    result = result.format(formatText1);
                } 
            }

            var element = $("span#{0}".format(id));            
            if(result != null) {
                element.html(element.html() + result);
            }
            element.show();

            return result;
        }

        //handle error logging + HTML output
        var LogError = function(postfixId) {
            var id = "{0}{1}".format(settings.outputPrefix,postfixId);
            var text = null;
            if (arguments.length > 1) {                
                if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
            }

            settings.logCallback().error(settings.logType,GetFolderName(),text);
            settings.errorsCounter += 1;
            return false;
        }

        //Handle warn logging
        var LogWarn = function(postfixId) {
            var id = "{0}{1}".format(settings.outputPrefix,postfixId);
            var text = null;
            if (arguments.length > 1) {                
                if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
            }

            settings.logCallback().warn(settings.logType,GetFolderName(),text);
        }
        
        //Handle info logging
        var LogInfo = function(postfixId) {
            var id = "{0}{1}".format(settings.outputPrefix,postfixId);
            var text = null;
            if (arguments.length > 1) {                
                if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
            }

            settings.logCallback().info(settings.logType,GetFolderName(),text);
        }

        // validate required labels values
        var ValidateLabelValues = function (fileName,lines) {
            var result = true;
            settings.metadataLabels.forEach(label => {
                var index = lines.indexOf(label);
                index += 1;
                if(label === settings.metadataLabels[0] || label === settings.metadataLabels[1] || label === settings.metadataLabels[2]) {
                    if(lines[index].trim() === "") {
                        result = LogError("-CheckMetadata-FileLabel-ValueRequired-Error",fileName,label);
                    }
                    else {
                        if(lines[index + 1].trim() !== "") {
                            result = LogError("-CheckMetadata-FileLabel-ValueMax-Error",fileName,label);
                        }
                    }                    
                }
                if(label === settings.metadataLabels[5] && lines[index].trim() === "") {
                    result = LogError("-CheckMetadata-FileLabel-ValueRequired-Error",fileName,label);
                    settings.errorStop = true;
                }
            });
            return result;
        }

        //Validate Metadata Labels required & orders
        var ValidateLabels = function (fileName,lines) {
            var result = true;
            var orderError = false;
            var prevIndex = -1;
            settings.metadataLabels.forEach(label => {
                var index = lines.indexOf(label);
                if(index < 0) {
                    result = LogError("-CheckMetadata-FileLabelRequired-Error",fileName,label);
                }
                else {
                    if(prevIndex > index) { orderError = true; }
                    prevIndex = index;
                    index += 1;
                    if(lines.indexOf(label,index) > -1) {
                        result = LogError("-CheckMetadata-FileLabelMax-Error",fileName,label);
                    }
                }
            });
            if(orderError) {
                result = LogError("-CheckMetadata-FileLabelsOrder-Error",fileName);
            }
            return result;
        }

        //validate Metadata file
        var ValidateMetadata = function (metadataFilePath,fileName) {
            var result = true;
            var data = fs.readFileSync(metadataFilePath);
            if(data != null) {
                data = data.toString();
                if(data != null && data !== "") {
                    var lines = data.split("\r\n");
                    lines[0] = lines[0].trim();
                    if(!ValidateLabels(fileName,lines)) 
                    { 
                        result = false; 
                        settings.errorStop = true;
                    }
                    else {
                        if(!ValidateLabelValues(fileName,lines)) { result = false; }
                    }
                }
                else {
                    result = LogError("-CheckMetadata-FileEmpty-Error",fileName);
                }
            }
            else {
                result = LogError("-CheckMetadata-FileEmpty-Error",fileName);
            }
            return result;
        }

        //loop Data folder's table & Metadata files
        var ValidateData = function () {
            var result = true;
            var destPath = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.deliveryPackagePath,settings.dataPathPostfix) : "{0}/{1}".format(settings.deliveryPackagePath,settings.dataPathPostfix); 
            fs.readdirSync(destPath).forEach(folder => {
                var metadataFilePath = (destPath.indexOf("\\") > -1) ? "{0}\\{1}\\{1}.txt".format(destPath,folder) : "{0}/{1}/{1}.txt".format(destPath,folder);                 
                console.log("validate metadata file: {0}".format(metadataFilePath));
                var charsetMatch = chardet.detectFileSync(metadataFilePath);
                var folders = metadataFilePath.getFolders();
                var fileName = folders[folders.length - 1];
                if(charsetMatch !== "UTF-8") {
                    result = LogError("-CheckMetadata-FileEncoding-Error",fileName);
                } 
                else {
                    if(!ValidateMetadata(metadataFilePath,fileName)) { result = false; }
                }                               
            });
            if(result) { LogInfo("-CheckMetadata-Ok",null); }
            return result; 
        }

        //start flow validation
        var Validate = function () {
            console.log(`metadata selected path: ${settings.deliveryPackagePath}`); 
            try 
            {
                var folderName = GetFolderName();
                settings.logCallback().section(settings.logType,folderName,settings.logStartSpn.innerHTML);            
                ValidateData();
                if(settings.errorsCounter === 0) {
                    settings.logCallback().section(settings.logType,folderName,settings.logEndNoErrorSpn.innerHTML);
                } else {
                    settings.logCallback().section(settings.logType,folderName,settings.logEndWithErrorSpn.innerHTML);
                } 
                return settings.logCallback().commit(settings.deliveryPackagePath);               
            }
            catch(err) 
            {
                HandleError(err);
            }
            return null;
        }

        var AddEvents = function (){
            console.log('events ');
        }

        //Model interfaces functions
        Rigsarkiv.Nemesis.MetaData = {        
            initialize: function (logCallback,outputErrorId,logStartId,logEndNoErrorId,logEndWithErrorId,outputPrefix) {            
                settings.logCallback = logCallback;
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.logStartSpn = document.getElementById(logStartId);
                settings.logEndNoErrorSpn = document.getElementById(logEndNoErrorId);  
                settings.logEndWithErrorSpn = document.getElementById(logEndWithErrorId);
                settings.outputPrefix = outputPrefix;
                AddEvents();
            },
            callback: function () {
                return { 
                    validate: function(path,outputText) 
                    { 
                        settings.deliveryPackagePath = path;
                        settings.outputText = outputText;
                        return Validate();
                    }  
                };
            }
        };
    }(jQuery);
}(jQuery);