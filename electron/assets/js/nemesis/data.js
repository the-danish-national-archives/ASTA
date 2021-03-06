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
        const path = require('path');
        const os = require('os');
        const chardet = require('chardet');
        const csv = require('fast-csv');
        const { spawn } = require('child_process');

        const codeListPattern = /^(\.[a-z])$|^([A-Z])$/;
        const emptyPattern = /^(\.)$|^(NaN)$|^(NA)$|^(n\.a\.)$|^(<none>)$|^(NULL)$/;
        const doubleApostrophePattern1 = /^"([\w\W\s]*)"$/;
        const doubleApostrophePattern2 = /(")/g;
        const doubleApostrophePattern3 = /(["]{2,2})/g
        const datatypeDecimal = /^(\%([0-9]+)\.([0-9]+)g)$/;
        const csvSplitPattern = /(?:^|;)(\"(?:[^\"]+|\"\")*\"|[^;]*)/g;
        const errorsMax = 40;
        const warningMax = 100;
        const maxInt = 2147483647;
        const minInt = -2147483648;
        const maxDecimal = 79228162514264337593543950335.79228162514264337593543950335;
        const minDecimal = -79228162514264337593543950335.79228162514264337593543950335;

        //private data memebers
        var settings = { 
            outputErrorSpn: null,
            outputErrorText: null,
            output: null,
            extraInfo: null,
            rightsCallback: null,
            logCallback: null,
            selectDirBtn: null,
            validateBtn: null,
            confirmationSpn: null,
            extraInfoTitleSpn: null,
            validateRowsText: null,
            deliveryPackagePath: null,
            ConvertBtn: null,
            logType: "data",
            fileName: null,
            metadataFileName: null,
            table: null,
            rowIndex: 0,
            dataFiles: [],
            runIndex: -1,
            dataPathPostfix: "Data",
            logResult: null,
            metadata: [],
            data: [],
            errors: 0,
            tableErrors: 0,
            tableRows: 0,
            tableWarnings: {},
            totalErrors: 0,
            appliedRegExp: -1,
            separator: ';',
            errorStop: false,
            convertStop: false,
            scriptPath: "./assets/scripts/{0}",
            resourceWinPath: "resources\\{0}",
            converterFileName: "AthenaForm.exe",
            metadataFilePostfix: "{0}.json",
            linkId: "nemesis-output-link-data-{0}",
            linkElement: "<a id=\"{0}\" href=\"#{1}\" class=\"nemesisLink\">{1}</a>",
            links: 0
        }

         // Render element's text
         var RenderElement = function(text) {
            var linkId = null;
                var element = document.createElement('span');
                $(element).html(text);
                if(element.firstChild != null && element.firstChild.firstChild != null) {
                    linkId = settings.linkId.format(settings.links);
                    settings.links += 1;
                    element.firstChild.insertAdjacentHTML("afterbegin", settings.linkElement.format(linkId,element.firstChild.firstChild.id));
                }
                settings.output.append($(element).html());
                if(linkId != null) {
                    document.getElementById(linkId).addEventListener('click', (event) => {
                        $("a[id^='nemesis-output-link-']").removeClass("nemesisLinkBold");
                        var contentId = event.srcElement.href.split("#")[1];
                        settings.extraInfoTitleSpn.innerHTML = Rigsarkiv.Language.callback().getValue("nemesis-extraInfo-SPAN").format(contentId);
                        var content =  document.getElementById(contentId);
                        $(event.srcElement).toggleClass("nemesisLinkBold");
                        settings.extraInfo.html($(content).html());
                    })
                }      
        }

        // View Element by id & return texts
        var ViewElement = function(id,formatText1,formatText2,formatText3,formatText4, formatText5, formatText6) {
            var result = Rigsarkiv.Language.callback().getValue(id);
            if(result == null) { return null; } 
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
            if(result != null) {
                RenderElement(result);              
            }
            return result;
        }

        //handle error logging + HTML output
        var LogError = function(id) {
            var text = null;
            if (arguments.length > 1) {                
                if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4]); }
                if(arguments.length === 6) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5]); }
                if(arguments.length === 7) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5],arguments[6]); }
            }
            if(text != null) { settings.logCallback().error(settings.logType,GetFolderName(),text); }
            settings.errors += 1;
            settings.totalErrors += 1;
            settings.tableErrors += 1;
            return false;
        }

        //Handle warn logging
        var LogWarn = function(id) {
            if(!settings.tableWarnings.hasOwnProperty(id)) {
                settings.tableWarnings[id] = 0;
            }
            if(settings.tableWarnings[id] < warningMax) {
                var text = null;
                if (arguments.length > 1) {                
                    if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                    if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                    if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
                    if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4]); }
                    if(arguments.length === 6) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5]); }
                    if(arguments.length === 7) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5],arguments[6]); }
                }
                if(text != null) { settings.logCallback().warn(settings.logType,GetFolderName(),text); }
            }                
            settings.tableWarnings[id] += 1;
            return true;
        }
        
        //Handle info logging
        var LogInfo = function(id) {
            var text = null;
            if (arguments.length > 1) {                
                if(arguments.length === 2) { text = ViewElement(id,arguments[1],null,null); }
                if(arguments.length === 3) { text = ViewElement(id,arguments[1],arguments[2],null); }
                if(arguments.length === 4) { text = ViewElement(id,arguments[1],arguments[2],arguments[3]); }
                if(arguments.length === 5) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4]); }
                if(arguments.length === 6) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5]); }
                if(arguments.length === 7) { text = ViewElement(id,arguments[1],arguments[2],arguments[3],arguments[4],arguments[5],arguments[6]); }
            }
            if(text != null) { settings.logCallback().info(settings.logType,GetFolderName(),text); }
            return true;
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

        // get table folder name by path
        var GetTableFolderName = function(filePath) {
            var folders = filePath.getFolders();
            return folders[folders.length - 2];
        } 

        //Validate Time value
        var ValidateTime = function (dataValue, regExp, variable) {
            var result = true;
            var matches = dataValue.match(regExp);
            var date = new Date(2019,01,01,parseInt(matches[1]),parseInt(matches[2]),parseInt(matches[3]));
            if(date.getFullYear() !== 2019 || date.getMonth() !== 1 || date.getDate() !== 1 || date.getHours() !== parseInt(matches[1]) || date.getMinutes() !== parseInt(matches[2]) || date.getSeconds() !== parseInt(matches[3])) {
                result = LogError("nemesis-processing-CheckData-FileRow-ColumnsTimeValue-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);
            }
            return result;
        }

        // Check for leading 0 numbers if the value is not just one digit.
        var ValidateInt = function (dataValue, regExp, variable) {
            var result = true;
            var matches = dataValue.match(regExp);
            var parsedValue = parseInt(matches[0]);
            if(isNaN(parsedValue)){
                result = LogError("nemesis-processing-CheckData-FileRow-ColumnsIntValue-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);  
            }
            else {
                if(parsedValue > maxInt || parsedValue < minInt) {
                    result = LogError("nemesis-processing-CheckData-FileRow-ColumnsInt-ValueRange-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);
                }
                else {
                    if (matches[0].length > 1) {
                        if (matches[0].charAt(0) === '-' || matches[0].charAt(0) === '+') {
                            if (matches[1] === '0') {
                                result = LogWarn("nemesis-processing-CheckData-FileRow-ColumnsIntValue-Warning", settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, variable.format);
                            }
                        } else {
                            if (matches[0].charAt(0) === '0') {
                                result = LogWarn("nemesis-processing-CheckData-FileRow-ColumnsIntValue-Warning", settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, variable.format);
                            }
                        }
                    }
                }                
            }
            return result;
        }

        //Validate Decimal value
        var ValidateDecimal = function (dataValue, regExp, variable) {
            var result = true;
            var matches = dataValue.match(regExp);
            var parsedValue = parseFloat(matches[0].replace(",","."));
            if(isNaN(parsedValue)){
                result = LogError("nemesis-processing-CheckData-FileRow-ColumnsDecimalValue-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);  
            }
            else {
                if(parsedValue > maxDecimal || parsedValue < minDecimal) {
                    result = LogError("nemesis-processing-CheckData-FileRow-ColumnsDecimal-ValueRange-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);
                }
                else {
                    if(variable.appliedRegExp !== settings.appliedRegExp && variable.appliedRegExp < 2 && settings.appliedRegExp < 2) {
                        result = LogError("nemesis-processing-CheckData-FileRow-ColumnsDecimal-InexpedientValue-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);
                    }
                }                
            }
            return result;
        }

        //Validate Date value
        var ValidateDate = function (dataValue, regExp, variable) {
            var result = true;
            var matches = dataValue.match(regExp);
            var date = new Date(parseInt(matches[1]),parseInt(matches[2]) - 1,parseInt(matches[3]));
            if(date.getFullYear() !== parseInt(matches[1]) || date.getMonth() !== (parseInt(matches[2]) - 1) || date.getDate() !== parseInt(matches[3])) {
                result = LogError("nemesis-processing-CheckData-FileRow-ColumnsDateValue-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);
            }
            else {
                if(variable.appliedRegExp !== settings.appliedRegExp) {
                    result = LogError("nemesis-processing-CheckData-FileRow-ColumnsDate-InexpedientValue-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);
                }
            }
            return result;
        }

        //Conver Month MMM to number
        var GetMonth = function (monthValue) {
            result = monthValue;
            if(isNaN(monthValue)) {
                switch (monthValue.toUpperCase()) {
                    case 'JAN': result = "1";break;
                    case 'FEB': result = "2";break;
                    case 'MAR': result = "3";break;
                    case 'APR': result = "4";break;
                    case 'MAY': result = "5";break;
                    case 'JUN': result = "6";break;
                    case 'JUL': result = "7";break;
                    case 'AUG': result = "8";break;
                    case 'SEP': result = "9";break;
                    case 'OCT': result = "10";break;
                    case 'NOV': result = "11";break;
                    case 'DEC': result = "12";break;
                }
            }
            return result;
        }

        //Validate Date time value
        var ValidateDateTime = function (dataValue, regExp, variable) {
            var result = true;
            var matches = dataValue.match(regExp);
            var year = parseInt(matches[1]);
            var month = parseInt(GetMonth(matches[2])) - 1;
            var day = parseInt(matches[3]);
            var hour = parseInt(matches[4]);
            var minute = parseInt(matches[5]);
            var second = parseInt(matches[6]);
            if(regExp.toString() === "/^([0-9]{2,2})-([a-zA-Z]{3,3})-([0-9]{4,4})\\s([0-9]{2,2}):([0-9]{2,2}):([0-9]{2,2})$/") {
                year = parseInt(matches[3]);
                day = parseInt(matches[1]);
            }            
            var date = new Date(year,month,day,hour,minute,second);
            if(date.getFullYear() !== year || date.getMonth() !== month || date.getDate() !== day || date.getHours() !== hour || date.getMinutes() !== minute || date.getSeconds() !== second) {
                result = LogError("nemesis-processing-CheckData-FileRow-ColumnsDateTimeValue-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);
            }
            else {
                if(variable.appliedRegExp !== settings.appliedRegExp) {
                    result = LogError("nemesis-processing-CheckData-FileRow-ColumnsDateTime-InexpedientValue-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);
                }
            }
            return result;
        }

        //strip text double Apostrophe
        var ApostropheNormalizer = function(dataValue,variable) {
            var result = dataValue;
            var isReferenced = false;
            if(result.indexOf("\"") > -1 && doubleApostrophePattern1.test(result)) {
                result = result.match(doubleApostrophePattern1)[1];
                result = result.replace(/""/g, "\"");
            }            
            if(variable != null && result.length > 0 && (result[0] === " " || result[result.length - 1] === " ")) {
                settings.table.references.forEach(reference => {
                    if(reference.refKeys.includes(variable.name)) { isReferenced = true; }
                });
                if(variable.isKey || isReferenced) {
                    result = LogError("nemesis-processing-CheckData-FileRow-ColumnsString-ValueBlankCharacters-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name);
                }
                else {
                    result = LogWarn("nemesis-processing-CheckData-FileRow-ColumnsString-ValueBlankCharacters-Warning",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name);
                }                
            }
            return result;
        }

        //Validate String
        var ValidateString = function (dataValue, regExp, variable) {
            var result = false;
            if(dataValue.indexOf("\"") > -1) {
                if(doubleApostrophePattern1.test(dataValue)) {
                    var innerText = dataValue.match(doubleApostrophePattern1)[1];
                    doubleApostrophePattern2.lastIndex = 0;
                    if(doubleApostrophePattern2.test(innerText)) {
                        var singleApostrophes = innerText.match(doubleApostrophePattern2).length;
                        if((singleApostrophes % 2) === 0){
                            doubleApostrophePattern3.lastIndex = 0;
                            if(doubleApostrophePattern3.test(innerText)) 
                            { 
                                var doubleApostrophes = innerText.match(doubleApostrophePattern3).length;
                                result = ((doubleApostrophes * 2) === singleApostrophes); 
                            }
                        }
                    }
                    else {
                        result = true;   
                    }
                }
                if(!result) {
                    settings.errorStop = true;
                    result = LogError("nemesis-processing-CheckData-FileRow-ColumnsString-ValueApostrophe-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name,dataValue);
                }
            }
            else {
                result = true;
            }
            return result;
        }

        // Validate single data cell value format
        var ValidateValue = function (dataValue, regExp, variable) {
            var result = true;
            switch (variable.type) {
                case 'String':
                    {
                        result = ValidateString(dataValue, regExp, variable);
                        if(result) { result = ValidateOptions(ApostropheNormalizer(dataValue,null),variable); }
                        if(result && emptyPattern.test(dataValue)) { LogWarn("nemesis-processing-CheckData-FileRow-ColumnsStringEmpty-Warning",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name,dataValue.replace("<","&lt;").replace(">","&gt;")); }
                    };break;
                case 'Int':
                    {
                        result = ValidateInt(dataValue, regExp, variable);
                        if(result) { result = ValidateOptions(dataValue,variable); }
                    };break;
                case 'Decimal':
                    {
                        result = ValidateDecimal(dataValue, regExp, variable);
                        if(result) { result = ValidateOptions(dataValue,variable); }
                    };break;
                case 'Date': 
                result = ValidateDate(dataValue, regExp, variable);
                    break;
                case 'DateTime':
                    result = ValidateDateTime(dataValue, regExp, variable);
                    break;
                case 'Time':
                    result = ValidateTime(dataValue, regExp, variable);
                    break;                
                default: break;
            }
            return result;
        }

        //Validate single data cell KODELISTE
        var ValidateOptions = function (dataValue,variable) {
            var result = true;
            if(variable.options != null && variable.options.length > 0) {
                var exist = false;
                variable.options.forEach(option => {
                    if(option.name === dataValue) { exist = true; }
                });
                if(!exist) {
                    result = LogWarn("nemesis-processing-CheckData-FileRow-ColumnsOptions-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue); 
                }
            }
            return result;
        }

        // Validate single data cell value format
        var ValidateFormat = function (dataValue,variable) {
            var result = true;
            switch (variable.type) {
                case 'String':
                    {
                        var regExSplit = variable.regExps[0].split(','); 
                        var length = regExSplit[1].split('}');
                        result = LogError("nemesis-processing-CheckData-FileRow-ColumnsStringType-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, variable.format,dataValue);
                    };break;
                case 'Int':
                    {
                        if(!codeListPattern.test(dataValue)) {
                            result = LogError("nemesis-processing-CheckData-FileRow-ColumnsIntType-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, variable.format,dataValue);
                        }
                        else 
                        {
                            result = ValidateOptions(dataValue,variable);
                            if(result && (variable.options == null || (variable.options != null && variable.options.length === 0))) {
                                result = LogWarn("nemesis-processing-CheckData-FileRow-ColumnsOptions-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);
                            }
                        }
                    };break;
                case 'Decimal':
                    {
                        if(!codeListPattern.test(dataValue)) {
                            result = LogError("nemesis-processing-CheckData-FileRow-ColumnsDecimalType-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, variable.format,dataValue);
                        }
                        else 
                        {
                            result = ValidateOptions(dataValue,variable);
                            if(result && (variable.options == null || (variable.options != null && variable.options.length === 0))) {
                                result = LogWarn("nemesis-processing-CheckData-FileRow-ColumnsOptions-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, dataValue);
                            }
                        }
                    };break;
                case 'Date':
                    result = LogError("nemesis-processing-CheckData-FileRow-ColumnsDateType-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, variable.format,dataValue);
                break;
                case 'DateTime':
                    result = LogError("nemesis-processing-CheckData-FileRow-ColumnsDateTimeType-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, variable.format,dataValue);
                break;
                case 'Time':
                    result = LogError("nemesis-processing-CheckData-FileRow-ColumnsTimeType-Error",settings.fileName,settings.metadataFileName, settings.rowIndex, variable.name, variable.format,dataValue);
                break;
                default: break;
            }
            return result;
        }

        //Validate row at CSV file
        var ValidateRow = function(dataRow) {
            var result = true;
            //console.logInfo(dataRow);
            for(var i = 0;i < dataRow.length;i++) {
                var patternMatch = false;
                var variable = settings.table.variables[i];
                if(dataRow[i] !== undefined && dataRow[i].trim() !== "") {                    
                    for(var j = 0;j < variable.regExps.length;j++) {
                        var patt = new RegExp(variable.regExps[j]);
                        var dataValue = (variable.type != "String") ? dataRow[i] : ApostropheNormalizer(dataRow[i],variable);
                        if(patt.test(dataValue)) {
                            settings.appliedRegExp = j;
                            if(variable.appliedRegExp === -1) { variable.appliedRegExp = j; }
                            if(!ValidateValue(dataRow[i], patt, variable)) { result = false; }
                            patternMatch = true;                            
                        }
                    }
                    if(!patternMatch) {
                        if(!ValidateFormat(dataRow[i],variable))  { result = false; }
                    }
                }
                else {
                    if(dataRow[i].trim() === "") { variable.nullable = true; } 
                } 
            }
            if(!result) { settings.errorStop = true; }
            return result;
        }

        // Validate CSV header row
        var ValidateHeader = function (data) {
            console.logInfo(`CSV headers: ${data.length}`,"Rigsarkiv.Nemesis.Data.ValidateHeader");
            var result = true;
            var variables = [];
            if(data.length === 1) { result = LogError("nemesis-processing-CheckData-FileSeprator-Error",settings.fileName); }
            else {                    
                settings.table.variables.forEach(variable => variables.push(variable.name));
                if(variables.length !== data.length) {
                    result = LogError("nemesis-processing-CheckData-FileHeaders-MatchColumns-Error",settings.fileName,settings.metadataFileName);
                }
                else {
                    var notExists = 0;
                    data.forEach(header => {
                        if(!variables.includes(header.trim())) { notExists++; }
                    });
                    if(notExists === data.length) {
                        result = LogError("nemesis-processing-CheckData-FileHeaders-MatchAll-Error",settings.fileName,settings.metadataFileName);
                    }
                    else {
                        data.forEach(header => {
                            if(!variables.includes(header.trim())) { result = LogError("nemesis-processing-CheckData-FileHeaders-MatchColumn-Error",settings.fileName,settings.metadataFileName,header); }
                        });  
                    }
                    if(result) {
                        for(var i = 0;i < data.length;i++) {
                            if(data[i].trim() !== variables[i]) { result = LogError("nemesis-processing-CheckData-FileHeaders-ColumnsOrder-Error",settings.fileName,settings.metadataFileName,data[i]);  }
                        }
                    }
                }
            }
            return result;
        }
        
        //Parse single row
        var ParseRow = function (data) {
            var result = [];
            csvSplitPattern.lastIndex = 0;
            var matches = data.match(csvSplitPattern);
            if(matches != null && matches.length > 0) {
                for(var i = 0;i < matches.length;i++) {
                    var value = matches[i];
                    if(value.indexOf(";") === 0) { value = value.substring(1); }
                    result.push(value);
                }
            }
            return result;
        }

        //Process Row
        var ProcessRow = function(data) {
            var result = true;
            var newData = data;
            result = (settings.table.variables.length === newData.length);
            if(!result) {
                var row = data.join(settings.separator);
                if(row.indexOf("\"") > -1) { //Reparsing of row if it contains double apstrof
                    newData = ParseRow(row);
                    result = (settings.table.variables.length === newData.length); 
                }
                if(settings.table.variables.length === (newData.length + 1)) { 
                    newData.push(""); 
                    result = true;
                }                                 
            }
            if(!result) { //less or more separators
                result = LogError("nemesis-processing-CheckData-FileRows-MatchLength-Error",settings.fileName,settings.rowIndex);
                settings.errorStop = true;
            }
            else {
                result = ValidateRow(newData); 
            }
            if(result && settings.data.length <= 100) { settings.data.push(newData); } 
            return result;
        }

        // Validate CSV header row
        var ValidateDataSet = function () {
            var dataFilePath = settings.dataFiles[settings.runIndex];
            fs.createReadStream(dataFilePath, { encoding:"utf8" })
            .pipe(csv({ delimiter: settings.separator, quote:null }))
            .on("data", function(data){  })
            .validate(function(data){
                var result = true;
                settings.rowIndex++;
                if(settings.rowIndex > 1) {
                    settings.confirmationSpn.innerHTML = settings.validateRowsText.format(settings.rowIndex - 1,settings.tableRows - 1,settings.fileName);
                }
                console.log("validate row: {0}".format(settings.rowIndex));
                if(settings.rowIndex === 1 && !ValidateHeader(data)) { 
                    settings.table.errorStop = true;
                    result = false; 
                }
                if(!settings.table.errorStop && settings.rowIndex > 1 && settings.tableErrors < errorsMax) {
                    
                    return ProcessRow(data);
                }
            })
            .on("end", function(){
                console.log("{0} data output: ".format(settings.fileName));
                console.log(settings.data);
                settings.confirmationSpn.innerHTML = "";
                if(settings.errorStop) { settings.table.errorStop = true; }
                settings.table.rows = settings.rowIndex - 1;
                ProcessDataSet();
            });
        }

        //Validate data file encoding
        var ValidateEncoding = function() {
            var dataFilePath = settings.dataFiles[settings.runIndex];
            var charsetMatch = chardet.detectFileSync(dataFilePath);
            if(charsetMatch !== "UTF-8") {
               
            }
            settings.confirmationSpn.innerHTML = "";
        }

        //Process all data files
        var ProcessDataSet = function() {
            settings.runIndex = settings.runIndex + 1;
            if(settings.runIndex < settings.dataFiles.length) {
                var dataFilePath = settings.dataFiles[settings.runIndex];
                var logTableText = Rigsarkiv.Language.callback().getValue("nemesis-output-metadata-logTable").format(GetTableFolderName(dataFilePath));
                settings.output.append(logTableText);
                settings.logCallback().section(settings.logType,GetFolderName(),logTableText);
                settings.fileName = GetFileName(dataFilePath);
                settings.metadataFileName =  "{0}.txt".format(settings.fileName.substring(0,settings.fileName.indexOf(".")));
                settings.table = GetTableData();
                settings.rowIndex = 0;
                settings.data = [];
                settings.tableErrors = 0;
                settings.tableWarnings = {}; 
                settings.errorStop = false;
                console.logInfo(`validate: ${dataFilePath}`,"Rigsarkiv.Nemesis.Data.ProcessDataSet");
                settings.tableRows = 0;                
                 fs.createReadStream(dataFilePath)
                .on('data', function(chunk) {
                  for (i=0; i < chunk.length; ++i)
                    if (chunk[i] == 10) settings.tableRows++;
                })
                .on('end', function() {
                    console.logInfo(`total rows: ${settings.tableRows}`,"Rigsarkiv.Nemesis.Data.ProcessDataSet");                    
                    ValidateDataSet();
                });                
            }
            else {
                CommitLog();
            }
        }

        //loop Data folder's table & data files
        var ValidateData = function () {
            var destPath = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.deliveryPackagePath,settings.dataPathPostfix) : "{0}/{1}".format(settings.deliveryPackagePath,settings.dataPathPostfix); 
            fs.readdirSync(destPath).forEach(folder => {
                var dataFilePath = (destPath.indexOf("\\") > -1) ? "{0}\\{1}\\{1}.csv".format(destPath,folder) : "{0}/{1}/{1}.csv".format(destPath,folder);                 
                if(fs.existsSync(dataFilePath)) {
                    console.logInfo("validate data file: {0}".format(dataFilePath),"Rigsarkiv.Nemesis.Data.ValidateData");                    
                    settings.fileName = GetFileName(dataFilePath);                                       
                    settings.metadataFileName =  "{0}.txt".format(settings.fileName.substring(0,settings.fileName.indexOf(".")));
                    settings.table = GetTableData();
                    if(settings.table != null && !settings.table.errorStop) { settings.dataFiles.push(dataFilePath); }                                                              
                }
                else {
                    console.logInfo("None exist Data file path: {0}".format(dataFilePath),"Rigsarkiv.Nemesis.Data.ValidateData");
                }                              
            });
            if(settings.dataFiles.length > 0) 
            { 
                settings.validateRowsText = Rigsarkiv.Language.callback().getValue("nemesis-output-ValidateRows");
                ProcessDataSet(); 
            }            
        }

        //commit end all validation 
        var CommitLog = function () {            
                var folderName = GetFolderName();
                var enableData = false;
                var enableConvert = !settings.convertStop;
                settings.metadata.forEach(table => {
                    if(table.errorStop) { enableConvert = false; }
                    else { enableData = true; }
                });
                if(settings.errors === 0) {
                    LogInfo("nemesis-processing-CheckData-Ok",null);
                    settings.logCallback().section(settings.logType,folderName,Rigsarkiv.Language.callback().getValue("nemesis-output-data-logEndNoError"));
                } else {
                    LogInfo(enableData ? "nemesis-processing-CheckData-Warning" : "nemesis-processing-CheckData-ErrorStop",null);
                    settings.logCallback().section(settings.logType,folderName,enableData ? Rigsarkiv.Language.callback().getValue("nemesis-output-data-logEndWithError") : Rigsarkiv.Language.callback().getValue("nemesis-output-data-logEndWithErrorStop"));
                }                                
                if(enableConvert) {
                    settings.confirmationSpn.innerHTML = settings.totalErrors > 0 ? Rigsarkiv.Language.callback().getValue("nemesis-output-ConvertWarningEnabled") : Rigsarkiv.Language.callback().getValue("nemesis-output-ConvertEnabled");
                }
                else {
                    settings.confirmationSpn.innerHTML = Rigsarkiv.Language.callback().getValue("nemesis-output-ConvertDisabled");
                }
                settings.logResult = settings.logCallback().commit(settings.deliveryPackagePath,settings.confirmationSpn.innerHTML);
                if(settings.rightsCallback().isAdmin && enableConvert) {
                    var destPath = (settings.logResult.filePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.logResult.filePath,settings.metadataFilePostfix.format(folderName)) : "{0}/{1}".format(settings.logResult.filePath,settings.metadataFilePostfix.format(folderName)); 
                    fs.writeFileSync(destPath, JSON.stringify(settings.metadata)); 
                        settings.ConvertBtn.hidden = false; 
                }
                settings.selectDirBtn.disabled = false;
                settings.validateBtn.disabled = false;
                console.logInfo(`total errors: ${settings.totalErrors}`,"Rigsarkiv.Nemesis.Data.Validate");
                console.log("metadata output: ");
                console.log(settings.metadata);
        }

        //start flow validation
        var Validate = function () {
            console.logInfo(`data selected path: ${settings.deliveryPackagePath}`,"Rigsarkiv.Nemesis.Data.Validate"); 
            try 
            {         
                LogInfo("nemesis-processing-CheckData-Start",null);       
                settings.logCallback().section(settings.logType,GetFolderName(),Rigsarkiv.Language.callback().getValue("nemesis-output-data-logStart"));            
                ValidateData(); 
                if(settings.dataFiles.length === 0) 
                { 
                    CommitLog();  
                }                                                             
            }
            catch(err) 
            {
                if(settings.fileName != null) {
                    console.logInfo(`Failed file: ${settings.fileName}`,"Rigsarkiv.Nemesis.Data.Validate");
                    console.logInfo(`Failed row: ${settings.rowIndex}`,"Rigsarkiv.Nemesis.Data.Validate");
                }
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Nemesis.Data.Validate");
            }
            return settings.logResult;
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.ConvertBtn.addEventListener('click', (event) => {
                var converterFilePath = settings.scriptPath.format(settings.converterFileName);        
                if(!fs.existsSync(converterFilePath)) {
                    var rootPath = null;
                    if(os.platform() == "win32") {
                        rootPath = path.join('./');
                        converterFilePath = path.join(rootPath,settings.resourceWinPath.format(settings.converterFileName));
                    }
                    if(os.platform() == "darwin") {
                        var folders =  __dirname.split("/");
                        rootPath = folders.slice(0,folders.length - 3).join("/");
                        converterFilePath = "{0}/{1}".format(rootPath,settings.converterFileName);
                    }
                }
                var folderName = GetFolderName();   
                var destPath = (settings.logResult.filePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.logResult.filePath,settings.metadataFilePostfix.format(folderName)) : "{0}/{1}".format(settings.logResult.filePath,settings.metadataFilePostfix.format(folderName)); 
                var converter = spawn(converterFilePath, [destPath]);
                converter.stdout.on('data', (data) => console.logInfo(`stdout: ${data}`,"Rigsarkiv.Nemesis.Data.AddEvents"));                  
                converter.stderr.on('data', (data) => (new Error(data).Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Nemesis.Data.AddEvents")));
                converter.on('close', (code) => console.logInfo(`converter process exited with code ${code}`,"Rigsarkiv.Nemesis.Data.AddEvents"));
            });
        }

        //Model interfaces functions
        Rigsarkiv.Nemesis.Data = {
            initialize: function (rightsCallback,logCallback,outputErrorId,outputId,selectDirectoryId,validateId,confirmationId,convertId,extraInfoId,extraInfoTitleId) { 
                settings.rightsCallback = rightsCallback;
                settings.logCallback = logCallback;
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.output = $("#" + outputId);
                settings.ConvertBtn = document.getElementById(convertId);
                settings.selectDirBtn = document.getElementById(selectDirectoryId);
                settings.validateBtn = document.getElementById(validateId);
                settings.confirmationSpn  = document.getElementById(confirmationId);
                settings.extraInfoTitleSpn = document.getElementById(extraInfoTitleId);
                settings.confirmationSpn.innerHTML = "";
                settings.extraInfo = $("#" + extraInfoId);
                AddEvents();
            },
            callback: function () {
                return { 
                    validate: function(path,metadata,errors,convertStop) 
                    { 
                        settings.deliveryPackagePath = path;
                        settings.metadata = metadata;
                        settings.convertStop = convertStop;                        
                        settings.runIndex = -1;
                        settings.dataFiles = [];
                        settings.ConvertBtn.hidden = true;
                        settings.errors = 0;                        
                        settings.totalErrors = errors;
                        settings.links = 0;
                        return Validate();
                    }  
                };
            }
        };
    }(jQuery);
}(jQuery);