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
        const rowErrorsMax = 40;
        
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
            data: [],
            rowErrors: null
        }

        //output system error messages
        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputErrorSpn.hidden = false;
            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);
        }

        // View Element by id & return texts
        var ViewElement = function(id,formatText1,formatText2,formatText3,formatText4, formatText5, formatText6) {
            var result = settings.outputText[id];
            if(formatText1 != null) { 
                if(formatText2 != null) {
                    if(formatText3 != null) {
                        if(formatText4 != null){
                            if(formatText5 != null){
                                if(formatText6 != null){
                                    result = result.format(formatText1,formatText2,formatText3,formatText4,formatText5,formatText6);    
                                }
                                else {
                                    result = result.format(formatText1,formatText2,formatText3,formatText4,formatText5);
                                }
                            }
                            else {
                                result = result.format(formatText1,formatText2,formatText3,formatText4);
                            }
                        }
                        else {
                            result = result.format(formatText1,formatText2,formatText3);
                        }
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
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4]); }
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5]); }
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5],arguments[6]); }
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
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4]); }
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5]); }
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5],arguments[6]); }
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
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4]); }
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5]); }
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5],arguments[6]); }
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

        //Validate CSV headers
        var ValidateHeaders = function(headers) {
            var result = true;
            var variables = [];
            settings.table.variables.forEach(variable => variables.push(variable.name));
            if(variables.length !== headers.length) {
                result = LogError("-CheckData-FileHeaders-MatchColumns-Error",settings.fileName,settings.metadataFileName);
            }
            else {
                var notExists = 0;
                headers.forEach(header => {
                    if(!variables.includes(header.trim())) { notExists++; }
                });
                if(notExists === headers.length) {
                    result = LogError("-CheckData-FileHeaders-MatchAll-Error",settings.fileName,settings.metadataFileName);
                }
                else {
                    headers.forEach(header => {
                       if(!variables.includes(header.trim())) {
                           result = LogError("-CheckData-FileHeaders-MatchColumn-Error",settings.fileName,settings.metadataFileName,header);
                        }
                    });  
                }
                if(result) {
                    for(var i = 0;i < headers.length;i++) {
                        if(headers[i].trim() !== variables[i]) {
                            result = LogError("-CheckData-FileHeaders-ColumnsOrder-Error",settings.fileName,settings.metadataFileName,headers[i]);  
                        }
                    }
                }
            }
            return result;
        }

        var ValidateTime = function (dataValue, regExp, dataType) {
            var matches = dataValue.match(regExp);
                    // Check hours valid value
                    if (parseInt(matches[1])) {
                        // Check if the value is within the allowed values for hour.
                        // var varTime = new Date(2019, 05, 03, parseInt(matches[1]), parseInt(matches[2]), parseInt(matches[3]));
                        // if not true -> result = false;
                    }
                    if (parseInt(matches[2])) {
                        // Check if the value is within the allowed values for minute.
                    }
                    if (parseInt(matches[3])) {
                        // Check if the value is within the allowed values for seconds.
                    }
                    return null; // not yet implemented;
        }

        var ValidateString = function (dataValue, regExp, dataType) {
            var matches = dataValue.match(regExp);
                    // Check valid format for String variables.
                    return null; // Not yet implemented.
        }

        var ValidateInt = function (dataValue, regExp, dataType) {
            var result = true;
            // Check valid format for Int variables.
            var matches = dataValue.match(regExp);
            // Check for leading 0 numbers if the value is not just one digit.
            if (matches[0].length > 1) {
                // Check if the first char in the value is either + or -
                if (matches[0].charAt(0) === '-' || matches[0].charAt(0) === '+') {
                    // Check if the first number in the value is 0
                    if (matches[1] === '0') {
                        result = false;
                    }
                } else {
                    // Check if the first number in the value is 0
                    if (matches[0].charAt(0) === '0') {
                        result = false;
                    }
                }
            }
            return result;
        }

        var ValidateDecimal = function (dataValue, regExp, dataType) {
            return null; // not yet implemented.
        }

        var ValidateDate = function (dataValue, regExp, dataType) {
            return null; // not yet implemented.
        }

        var ValidateDateTime = function (dataValue, regExp, dataType) {
            return null; // not yet implemented.
        }

        // Validate single data cell value format
        var ValidateFormat = function (dataValue, regExp, dataType) {
            var result = true;
            switch (dataType) {
                case 'Time':
                    result = ValidateTime(dataValue, regExp, dataType);
                    break;
                case 'String':
                    result = ValidateString(dataValue, regExp, dataType);
                    break;
                case 'Int':
                    result = ValidateInt(dataValue, regExp, dataType);
                    break;
                case 'Decimal':
                    result = ValidateDecimal(dataValue, regExp, dataType);
                    break;
                case 'Date':
                    result = ValidateDate(dataValue, regExp, dataType);
                    break;
                case 'DateTime':
                    result = ValidateDateTime(dataValue, regExp, dataType);
                    break;
                // Handling user defined values?
                default:
                    console.log('Variable did not have a recongnized type: ' + dataValue);
                    break;
            }
            return result;
        }

        //Validate row at CSV file
        var ValidateRow = function(data) {
            var result = true;
            console.log(Object.values(data));
            var dataRow = Object.values(data);
            for(var i = 0;i < dataRow.length;i++) {
                if(dataRow[i].trim() !== ""){
                        var patternMatch = false;
                        var variable = settings.table.variables[i];
                    variable.regExps.forEach(regex => {
                        var patt = new RegExp(regex);
                        if(patt.test(dataRow[i])){
                            patternMatch = ValidateFormat(dataRow[i], patt, variable.type);
                        }
                    });
                    if(!patternMatch) {
                        if(variable.type === "String") {
                            var regExSplit = variable.regExps[0].split(','); 
                            var length = regExSplit[1].split('}');
                            result = LogError("-CheckData-FileRow-ColumnsStringType-Error",settings.fileName, (settings.rowIndex + 2), variable.name, length[0]);
                        }
                        if (variable.type === "Int") {
                            result = LogWarn("-CheckData-FileRow-ColumnsIntType-Warning", settings.fileName, variable.name, settings.metadataFileName, variable.format);
                        }
                    }
                } else {
                    // Check codelist if value can be missing / empty
                    
                }
            }
            return result;
        }


        //Validate single CSV file
        var ValidateDataSet = function (dataFilePath) {
            var result = true;
            settings.rowErrors = 0;  
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
                if(result && settings.rowErrors <= rowErrorsMax) {
                    if(!ValidateRow(data)) {
                        settings.rowErrors++;
                    }
                    else {
                        settings.data.push(data);
                    }                    
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
                if(settings.rowErrors > 0) { result = false; }
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
                        if(settings.table != null && !settings.table.errorStop) { settings.dataFiles.push(dataFilePath); }                                                              
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