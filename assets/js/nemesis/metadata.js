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

        const startNumberPattern = /^([0-9])([a-zA-ZæøåÆØÅ0-9_]*)$/;
        const validFileNamePattern = /^([a-zA-ZæøåÆØÅ])([a-zA-ZæøåÆØÅ0-9_]*)$/;
        const reservedWordPattern = /^(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)$/i;
        const enclosedReservedWordPattern = /^(")(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)(")$/i;
        const titleMaxLength = 128;
            
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
            metadataLabels: ["SYSTEMNAVN","DATAFILNAVN","DATAFILBESKRIVELSE","NØGLEVARIABEL","REFERENCE","VARIABEL","VARIABELBESKRIVELSE","KODELISTE","BRUGERKODE"],
            data: []
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

        //get data JSON table object pointer by table file name
        var GetTableData = function (fileName) {
            var result = null;
            settings.data.forEach(table => {
                if(table.fileName === fileName) {
                    result = table;
                }
            });
            return result;
        }

        //validate variables Description, update output data json
        var ValidateVariablesDescription = function (fileName,lines,startIndex) {
            var result = true;
            var table = GetTableData(fileName);
            var i = startIndex;

            return result;
        }

        //validate variables, update output data json
        var ValidateVariables = function (fileName,lines,startIndex) {
            var result = true;
            var table = GetTableData(fileName);
            var i = startIndex;
            do {
                var expressions = lines[i].trim().split(" ");
                if(expressions.length >= 2 && expressions[0] !== "" && expressions[1] !== "") {
                    var variable = { "name":expressions[0], "format":expressions[1], "description":"" }
                    table.variables.push(variable)
                }
                else {
                    result = LogError("-CheckMetadata-FileVariables-RowRequiredInfo-Error",fileName,(i + 1));
                    settings.errorStop = true;
                }
                i++;
              }
              while (lines[i].trim() !== "");
            return result;
        }

        //validate DATAFILNAVN
        var ValidateTitle = function (fileName,title) {
            var result = true;
            if(result && startNumberPattern.test(title)) {
                result = LogError("-CheckMetadata-FileLabel-TitleNumber-Error",fileName,title);
            }
            if(result && !validFileNamePattern.test(title) && !enclosedReservedWordPattern.test(title)) {
                result = LogError("-CheckMetadata-FileLabel-TitleValidation-Error",fileName,title);
            }
            if(result && title.length > titleMaxLength) {
                result = LogError("-CheckMetadata-FileLabel-TitleLength-Error",fileName,title);
            }
            if(result && reservedWordPattern.test(title)) {
                result = LogError("-CheckMetadata-FileLabel-TitleReservedWord-Error",fileName,title);
            }
            return result;
        }

        // validate required labels values
        var ValidateLabelValues = function (fileName,lines) {
            var result = true;
            var validateVariables = true;
            settings.metadataLabels.forEach(label => {
                var index = lines.indexOf(label);
                index += 1;
                if(label === settings.metadataLabels[0] || label === settings.metadataLabels[1] || label === settings.metadataLabels[2]) {
                    if(lines[index].trim() === "") {
                        result = LogError("-CheckMetadata-FileLabel-ValueRequired-Error",fileName,label);
                    }
                    else {
                        if(label === settings.metadataLabels[1]) {
                            if(!ValidateTitle(fileName,lines[index].trim())) { result = false; }
                        }
                        if(lines[index + 1].trim() !== "") {
                            result = LogError("-CheckMetadata-FileLabel-ValueMax-Error",fileName,label);
                        }
                    }                    
                }
                if(label === settings.metadataLabels[5]) {
                    if(lines[index].trim() === "") {
                        result = LogError("-CheckMetadata-FileLabel-ValueRequired-Error",fileName,label);
                        settings.errorStop = true;
                    }
                    else {
                        validateVariables = ValidateVariables(fileName,lines,index);
                        if(!validateVariables) { result = false; }
                    }
                }
                if(label === settings.metadataLabels[6] && validateVariables) {

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
                if(fs.existsSync(metadataFilePath)) {
                    console.log("validate metadata file: {0}".format(metadataFilePath));
                    var charsetMatch = chardet.detectFileSync(metadataFilePath);
                    var folders = metadataFilePath.getFolders();
                    var fileName = folders[folders.length - 1];
                    if(charsetMatch !== "UTF-8") {
                        result = LogError("-CheckMetadata-FileEncoding-Error",fileName);
                    } 
                    else {
                        settings.data.push({ "fileName":fileName, "variables":[] })
                        if(!ValidateMetadata(metadataFilePath,fileName)) { result = false; }
                    } 
                }
                else {
                    console.log("None exist Metadata file path: {0}".format(metadataFilePath));
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
                console.log("output data: ");
                console.log(settings.data);
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