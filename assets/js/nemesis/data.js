/*
    Model is responsible for Validate Data CSV (Flow 3.0)
    initialize interface inputs: elements from <div class="formularContainer">
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Nemesis = Rigsarkiv.Nemesis || {},
    function (n) {
        const {ipcRenderer} = require('electron');
        const fs = require('fs');
        const chardet = require('chardet');
        const csv = require('csv-parser');
        
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
            logType: "data",
            fileName: null,
            metadataFileName: null,
            table: null,
            rowIndex: 0,
            dataFiles: [],
            runIndex: -1,
            endValidation: false,
            dataPathPostfix: "Data",
            logResult: null,
            metadata: [],
            data: []
        }

        //output system error messages
        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputErrorSpn.hidden = false;
            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);
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

        // get selected folder name 
        var GetFolderName = function() {
            var folders = settings.deliveryPackagePath.getFolders();
            return folders[folders.length - 1];
        }

        //get data JSON table object pointer by table file name
        var GetTableData = function () {
            var result = null;            
            settings.metadata.forEach(table => {
                if(table.fileName === settings.metadataFileName) {
                    result = table;
                }
            });
            return result;
        }

        // get file name by path
        var GetFileName = function(filePath) {
            var folders = filePath.getFolders();
            return folders[folders.length - 1];
        }

        var ProcessDataSet = function() {
            settings.runIndex = settings.runIndex + 1;
            if(settings.runIndex < settings.dataFiles.length) {
                var dataFilePath = settings.dataFiles[settings.runIndex];
                settings.endValidation = false;
                settings.fileName = GetFileName(dataFilePath);
                settings.metadataFileName =  "{0}.txt".format(settings.fileName.substring(0,settings.fileName.indexOf(".")));
                settings.table = GetTableData();
                settings.rowIndex = 0;
                settings.data = [];
                console.log(`validate: ${dataFilePath}`);
                ValidateDataSet(dataFilePath);
            }
            else {
                settings.endValidation = true;
            }
        }

        var ValidateHeaders = function(headers) {
            var result = true;
            var variables = [];
            settings.table.variables.forEach(variable => variables.push(variable.name));
            if(variables.length !== headers.length) {
                result = LogError("-CheckData-FileHeaders-MatchColumns-Error",settings.fileName,settings.metadataFileName);
            }
            else {

            }

            return result;
        }

        //Validate single CSV file
        var ValidateDataSet = function (dataFilePath) {
            var result = true;  
            fs.createReadStream(dataFilePath)
            .pipe(csv({ separator: ';', strict:true }))
            .on('headers', (headers) => {
                console.log(`CSV headers: ${headers.length}`);
                if(headers.length === 1) {
                    result = LogError("-CheckData-FileSeprator-Error",settings.fileName);
                }
                else {
                    if(!ValidateHeaders(headers)) { result = false; }
                }
            })
            .on('data', (data) => {
                if(result) {
                    //console.log(Object.values(data));
                    settings.data.push(data);
                    settings.rowIndex++;
                }
            })
            .on('error', (e) => { 
                if(e.message === "Row length does not match headers") {
                    result = LogError("-CheckData-FileRows-MatchLength-Error",settings.fileName,(settings.rowIndex + 2));
                }
                ProcessDataSet();
            })
            .on('end', () => { 
                console.log("data output: ");
                console.log(settings.data);
                ProcessDataSet();
            }); 
            return result;
        }

        //loop Data folder's table & data files
        var ValidateData = function () {
            var result = true;
            var destPath = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.deliveryPackagePath,settings.dataPathPostfix) : "{0}/{1}".format(settings.deliveryPackagePath,settings.dataPathPostfix); 
            fs.readdirSync(destPath).forEach(folder => {
                var dataFilePath = (destPath.indexOf("\\") > -1) ? "{0}\\{1}\\{1}.csv".format(destPath,folder) : "{0}/{1}/{1}.csv".format(destPath,folder);                 
                if(fs.existsSync(dataFilePath)) {
                    console.log("validate data file: {0}".format(dataFilePath));
                    var charsetMatch = chardet.detectFileSync(dataFilePath);
                    settings.fileName = GetFileName(dataFilePath);
                    settings.metadataFileName =  "{0}.txt".format(settings.fileName.substring(0,settings.fileName.indexOf(".")));
                    settings.table = GetTableData();
                    if(charsetMatch !== "UTF-8") {
                        result = LogError("-CheckData-FileEncoding-Error",settings.fileName);
                    } 
                    else {
                        if(!settings.table.errorStop) { settings.dataFiles.push(dataFilePath); }                                                              
                    } 
                }
                else {
                    console.log("None exist Data file path: {0}".format(dataFilePath));
                }                              
            });
            if(settings.dataFiles.length > 0) { ProcessDataSet(); }
            if(result) { LogInfo("-CheckData-Ok",null); }
            return result; 
        }

        var CommitLog = function () {
            console.log("endValidation: {0}".format(settings.endValidation));
            if(!settings.endValidation) {
                setTimeout(CommitLog, 100);
            }
            else {
                var folderName = GetFolderName();
                if(settings.errorsCounter === 0) {
                    settings.logCallback().section(settings.logType,folderName,settings.logEndNoErrorSpn.innerHTML);
                } else {
                    settings.logCallback().section(settings.logType,folderName,settings.logEndWithErrorSpn.innerHTML);
                }
                settings.logResult = settings.logCallback().commit(settings.deliveryPackagePath);
            }
        }
        //start flow validation
        var Validate = function () {
            console.log(`data selected path: ${settings.deliveryPackagePath}`); 
            try 
            {
                settings.logCallback().section(settings.logType,GetFolderName(),settings.logStartSpn.innerHTML);            
                ValidateData(); 
                CommitLog();                                              
            }
            catch(err) 
            {
                HandleError(err);
            }
            return settings.logResult;
        }

        var AddEvents = function (){
            console.log('events ');
        }

        //Model interfaces functions
        Rigsarkiv.Nemesis.Data = {
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
                    validate: function(path,outputText,metadata) 
                    { 
                        settings.deliveryPackagePath = path;
                        settings.outputText = outputText;
                        settings.metadata = metadata;
                        settings.endValidation = true;                        
                        settings.runIndex = -1;
                        settings.dataFiles = [];
                        return Validate();
                    }  
                };
            }
        };
    }(jQuery);
}(jQuery);