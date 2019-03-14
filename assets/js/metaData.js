/*
    Model is responsible for metadata inputs
    initialize interface inputs: elements from <div id="hybris-panel-metdata">
    create metadata.txt
*/
window.Rigsarkiv = window.Rigsarkiv || {},
    function (n) {

        const { ipcRenderer } = require('electron');
        const {shell} = require('electron');
        const fs = require('fs');
        const path = require('path');
        const os = require('os');

        const startNumberPattern = /^([0-9])([a-zA-ZæøåÆØÅ0-9_]*)$/;
        const validFileNamePattern = /^([a-zA-ZæøåÆØÅ])([a-zA-ZæøåÆØÅ0-9_]*)$/;
        const reservedWordPattern = /^(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)$/i;
        const enclosedReservedWordPattern = /^(")(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)(")$/i;
        const strLength = 128;

        var settings = {
            fileName: null,
            fileDescr: null,
            keyVar: null,
            foreignFileName: null,
            foreignKeyVarName: null,
            foreignFileRefVar: null,
            okBtn: null,
            outputOkSpn: null,
            outputOkText: null,
            okDataPath: null,
            outputErrorSpn: null,
            outputErrorText: null,
            outputNewExtractionSpn: null,
            outputNewExtractionText: null,            
            newExtractionBtn: null,
            nextBtn: null,
            outputNextSpn: null,
            extractionTab: null,
            indexFilesTab: null,
            fileNameReqTitle: null,
            fileNameReqText: null,
            fileDescrReqTitle: null,
            fileDescrReqText: null,
            numberFirstTitle: null,
            numberFirstText: null,
            illegalCharTitle: null,
            illegalCharText: null,
            fileNameLengthTitle: null,
            fileNameLengthText: null,
            fileNameReservedWordTitle: null,
            fileNameReservedWordText: null,
            outputCloseApplicationErrorTitle: null,
            outputCloseApplicationErrorText: null,
            informationPanel1: null,
            informationPanel2: null,
            indexFilesDescriptionSpn: null,
            indexFilesDescriptionText: null,
            referencesTbl: null,
            addReferenceBtn: null,
            referenceReqTitle: null,
            referenceReqText: null,
            contents: ["","","",""],
            references: [],
            isValidMetadata: true,
            metadataFileName: "{0}.txt",
            dataFileName: "{0}.csv",
            metadataTemplateFileName: "metadata.txt",
            scriptPath: "./assets/scripts/{0}",
            resourceWinPath: "resources/{0}"
        }

        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputErrorSpn.hidden = false;
            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);
        }

        var Reset = function () {
            settings.informationPanel1.hidden = false;
            settings.informationPanel2.hidden = true;
            settings.isValidMetadata = true;
            settings.outputErrorSpn.hidden = true;
            settings.nextBtn.hidden = true;
            settings.contents = ["","","",""];
        }        

        var GetMetaDataFileName = function() {
            return settings.metadataFileName.format(GetDataFolderName());
        }

        var GetDataFolderName = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            var folders = dataFolderPath.split("/");
            return folders[folders.length - 1];
        }

        var Cleanup = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            fs.readdir(dataFolderPath, (err, files) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    var hasError = false;
                    var fileName = GetDataFolderName();
                    var callback = settings.extractionCallback();
                    var dataFolderPath = callback.dataFolderPath;
                    files.forEach(file => {
                        if(file != settings.dataFileName.format(fileName) && file != settings.metadataFileName.format(fileName)) {
                            console.log("delete file : " + file);
                            try {
                                fs.unlinkSync("{0}/{1}".format(dataFolderPath,file));                                
                            }
                            catch(err) {
                                hasError = true;
                                HandleError(err);
                            }                            
                        }
                    });
                    if(hasError) {
                        ipcRenderer.send('open-error-dialog',settings.outputCloseApplicationErrorTitle.innerHTML,settings.outputCloseApplicationErrorText.innerHTML);
                    }
                    else {
                        settings.informationPanel1.hidden = true;
                        settings.informationPanel2.hidden = false;
                        var folders = callback.selectedStatisticsFilePath.normlizePath().split("/");
                        settings.nextBtn.hidden = false;
                        settings.outputOkSpn.innerHTML = settings.outputOkText.format(settings.dataFileName.format(fileName),settings.metadataFileName.format(fileName),folders[folders.length - 1]);
                        settings.okDataPath.innerHTML = callback.localFolderPath;
                        folders = dataFolderPath.split("/");
                        settings.outputNewExtractionSpn.innerHTML = settings.outputNewExtractionText.format(folders[folders.length - 3]);
                        settings.indexFilesDescriptionSpn.innerHTML = settings.indexFilesDescriptionText.format(folders[folders.length - 3]);
                    }                                                          
                }
            });
        }

        var RenameFile = function() {
            var folders = settings.extractionCallback().selectedStatisticsFilePath.normlizePath().split("/");
            var srcFileName = folders[folders.length - 1];
            srcFileName = srcFileName.substring(0,srcFileName.indexOf("."));
            var destFileName = GetDataFolderName();
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            console.log("rename " + srcFileName + " file to: {0}.csv".format(destFileName));
            fs.rename("{0}/{1}".format(dataFolderPath,settings.dataFileName.format(srcFileName)) ,"{0}/{1}".format(dataFolderPath,settings.dataFileName.format(destFileName)) , (err) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    console.log('Rename complete!');
                    Cleanup();
                }
              }); 
        }

        var GetReferences = function() {
            var result = "";
            var foreignKeyVarName = null;
            var foreignFileRefVar = null;
            settings.references.forEach(element => {
                foreignKeyVarName = (element[1] != null && element[1] !== "") ? "'{0}'".format(element[1]) : "";
                foreignFileRefVar = (element[2] != null && element[2] !== "") ? "'{0}'".format(element[2]) : "";
                result += "{0} {1} {2}\r\n".format(element[0],foreignKeyVarName,foreignFileRefVar);
            });
            return result;
        }

        var UpdateFile = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            var metadataFileName = GetMetaDataFileName();
            fs.readFile("{0}/{1}".format(dataFolderPath,metadataFileName), (err, data) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    var callback = settings.extractionCallback();
                    var dataFolderPath = callback.dataFolderPath;
                    var metadataFileName = GetMetaDataFileName();
                    var scriptType = callback.scriptType;
                    var keyVar = (settings.keyVar.value != null && settings.keyVar.value !== "") ? "{0}\r\n".format(settings.keyVar.value) : "";
                    var updatedData = data.toString().format(scriptType,settings.fileName.value,settings.fileDescr.value,keyVar,
                        GetReferences(),
                        settings.contents[0],settings.contents[1],settings.contents[2],settings.contents[3]);
                    fs.writeFile("{0}/{1}".format(dataFolderPath,metadataFileName), updatedData, (err) => {
                        if (err) {
                            HandleError(err);
                        }
                        else {
                            RenameFile();
                        }
                    });
                }
            });
        }

        var EnsureFile = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            var metadataFileName = GetMetaDataFileName();
            var metadataFilePath = settings.scriptPath.format(settings.metadataTemplateFileName);
            
            if(!fs.existsSync(metadataFilePath)) {
                var rootPath = null;
                if(os.platform() == "win32") {
                    rootPath = path.join('./');
                    metadataFilePath = path.join(rootPath,settings.resourceWinPath.format(settings.metadataTemplateFileName));
                }
                if(os.platform() == "darwin") {
                    var folders =  __dirname.split("/");
                    rootPath = folders.slice(0,folders.length - 3).join("/");
                    metadataFilePath = "{0}/{1}".format(rootPath,settings.metadataTemplateFileName);
                }
            }

            console.log(`copy ${settings.metadataTemplateFileName} file to: ${dataFolderPath}`);
            fs.copyFile(metadataFilePath, "{0}/{1}".format(dataFolderPath,metadataFileName), (err) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    var dataFolderPath = settings.extractionCallback().dataFolderPath;
                    var metadataFileName = GetMetaDataFileName();
                    console.log(metadataFileName + ' was copied to '+ dataFolderPath);
                    UpdateFile();
                }
            });
        }

        var EnsureData = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            fs.readdir(dataFolderPath, (err, files) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    var dataFolderPath = settings.extractionCallback().dataFolderPath;
                    console.log(`get texts contents: ${dataFolderPath}`);
                    files.forEach(file => {
                        var filePath = "{0}/{1}".format(dataFolderPath,file);
                        if(file.lastIndexOf("_VARIABEL.txt") > -1) {
                            settings.contents[0] = fs.readFileSync(filePath).toString();
                        }
                        if(file.lastIndexOf("_VARIABELBESKRIVELSE.txt") > -1) {
                            settings.contents[1] = fs.readFileSync(filePath).toString();
                        }
                        if(file.lastIndexOf("_KODELISTE.txt") > -1) {
                            settings.contents[2] = fs.readFileSync(filePath).toString();
                        }
                        if(file.lastIndexOf("_BRUGERKODE.txt") > -1) {
                            settings.contents[3] = fs.readFileSync(filePath).toString();
                        }
                    });
                    EnsureFile();
                }
            });
        }

        var ValidateFields = function() {
            if (settings.fileName.value === "") {
                ipcRenderer.send('open-error-dialog',settings.fileNameReqTitle.innerHTML,settings.fileNameReqText.innerHTML);
                settings.isValidMetadata = false;
            }
            if(settings.isValidMetadata && settings.fileDescr.value === "") {
                ipcRenderer.send('open-error-dialog',settings.fileDescrReqTitle.innerHTML,settings.fileDescrReqText.innerHTML);
                settings.isValidMetadata = false;
            }
            if (settings.isValidMetadata && startNumberPattern.test(settings.fileName.value)) {
                ipcRenderer.send('open-error-dialog',settings.numberFirstTitle.innerHTML,settings.numberFirstText.innerHTML);
                settings.isValidMetadata = false;
            }
            if (settings.isValidMetadata && !validFileNamePattern.test(settings.fileName.value)) {
                if(!enclosedReservedWordPattern.test(settings.fileName.value)) {
                    ipcRenderer.send('open-error-dialog',settings.illegalCharTitle.innerHTML,settings.illegalCharText.innerHTML);
                    settings.isValidMetadata = false;
                }
            }
            if (settings.isValidMetadata && settings.fileName.value.length > strLength) {
                ipcRenderer.send('open-error-dialog',settings.fileNameLengthTitle.innerHTML,settings.fileNameLengthText.innerHTML);
                settings.isValidMetadata = false;
            }
            if (settings.isValidMetadata && reservedWordPattern.test(settings.fileName.value)) {
                ipcRenderer.send('open-error-dialog',settings.fileNameReservedWordTitle.innerHTML,settings.fileNameReservedWordText.innerHTML);
                settings.isValidMetadata = false;
            }
        }

        var ResetExtraction = function() {
            settings.extractionCallback().reset();
            Reset();
            $("#{0} tr:not(:first-child)".format(settings.referencesTbl.id)).remove();
            settings.referencesTbl.hidden = true;
            settings.references = [];
            settings.fileName.value = "";
            settings.fileDescr.value = "";
            settings.keyVar.value = "";
            settings.foreignFileName.value = "";
            settings.foreignKeyVarName.value = "";
            settings.foreignFileRefVar.value = "";
        }

        var AddEvents = function () {
            settings.nextBtn.addEventListener('click', (event) => {
                settings.indexFilesTab.click();
            });
            settings.newExtractionBtn.addEventListener('click', (event) => {
                ResetExtraction();
                settings.extractionTab.click();
            });
            settings.okDataPath.addEventListener('click', (event) => {
                shell.openItem(settings.extractionCallback().localFolderPath);
            })
            settings.okBtn.addEventListener('click', function (event) {
                Reset();
                ValidateFields();
                if(settings.isValidMetadata) { EnsureData(); }
            })
            settings.addReferenceBtn.addEventListener('click', function (event) {
                if (settings.foreignFileName.value === "" && settings.foreignKeyVarName.value === "" && settings.foreignFileRefVar.value === "") {
                    ipcRenderer.send('open-error-dialog',settings.referenceReqTitle.innerHTML,settings.referenceReqText.innerHTML);
                }
                else {
                    var selectedReferences = [settings.foreignFileName.value,settings.foreignKeyVarName.value,settings.foreignFileRefVar.value];
                    settings.references.push(selectedReferences);
                    settings.referencesTbl.hidden = false;
                    $(settings.referencesTbl).append("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>".format(selectedReferences[0],selectedReferences[1],selectedReferences[2]));
                    settings.foreignFileName.value = "";
                    settings.foreignKeyVarName.value = "";
                    settings.foreignFileRefVar.value = "";
                }               
            });
        }

        Rigsarkiv.MetaData = {
            initialize: function (extractionCallback,metadataFileName,metadataFileNameDescription,metadataKeyVariable,metadataForeignFileName,metadataForeignKeyVariableName,metadataReferenceVariable,metdataOkBtn,inputFileNameRequired,inputNumberFirst,inputIllegalChar,outputOkId,okDataPathId,outputErrorId,outputNewExtractionId,newExtractionBtn,extractionTabId,outputNextId,nextBtn,indexFilesTabId,fileNameLengthId,fileNameReservedWordId,fileDescrReqId,informationPanel1Id,informationPanel2Id,indexFilesDescriptionId,outputCloseApplicationErrorPrefixId,referencesId,addReferenceBtn,referenceReqId) {
                settings.extractionCallback = extractionCallback;
                settings.fileName = document.getElementById(metadataFileName);
                settings.fileDescr = document.getElementById(metadataFileNameDescription);
                settings.keyVar = document.getElementById(metadataKeyVariable);
                settings.foreignFileName = document.getElementById(metadataForeignFileName);
                settings.foreignKeyVarName = document.getElementById(metadataForeignKeyVariableName);
                settings.foreignFileRefVar = document.getElementById(metadataReferenceVariable);
                settings.okBtn = document.getElementById(metdataOkBtn);
                settings.fileNameReqTitle = document.getElementById(inputFileNameRequired + "-Title");
                settings.fileNameReqText = document.getElementById(inputFileNameRequired + "-Text");
                settings.numberFirstTitle = document.getElementById(inputNumberFirst + "-Title");
                settings.numberFirstText = document.getElementById(inputNumberFirst + "-Text");
                settings.illegalCharTitle = document.getElementById(inputIllegalChar + "-Title");
                settings.illegalCharText = document.getElementById(inputIllegalChar + "-Text");
                settings.outputOkSpn = document.getElementById(outputOkId);
                settings.outputOkText = settings.outputOkSpn.innerHTML;
                settings.okDataPath = document.getElementById(okDataPathId);
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML; 
                settings.outputNewExtractionSpn = document.getElementById(outputNewExtractionId);
                settings.outputNewExtractionText = settings.outputNewExtractionSpn.innerHTML;
                settings.newExtractionBtn = document.getElementById(newExtractionBtn);
                settings.extractionTab = document.getElementById(extractionTabId);
                settings.outputNextSpn = document.getElementById(outputNextId); 
                settings.nextBtn = document.getElementById(nextBtn);
                settings.indexFilesTab = document.getElementById(indexFilesTabId);
                settings.fileNameLengthTitle = document.getElementById(fileNameLengthId + "-Title");
                settings.fileNameLengthText = document.getElementById(fileNameLengthId + "-Text");
                settings.fileNameReservedWordTitle = document.getElementById(fileNameReservedWordId + "-Title");
                settings.fileNameReservedWordText = document.getElementById(fileNameReservedWordId + "-Text");
                settings.fileDescrReqTitle = document.getElementById(fileDescrReqId + "-Title");
                settings.fileDescrReqText = document.getElementById(fileDescrReqId + "-Text");
                settings.informationPanel1 = document.getElementById(informationPanel1Id);
                settings.informationPanel2 = document.getElementById(informationPanel2Id);
                settings.indexFilesDescriptionSpn = document.getElementById(indexFilesDescriptionId);
                settings.indexFilesDescriptionText = settings.indexFilesDescriptionSpn.innerHTML;
                settings.outputCloseApplicationErrorTitle = document.getElementById(outputCloseApplicationErrorPrefixId + "-Title");
                settings.outputCloseApplicationErrorText = document.getElementById(outputCloseApplicationErrorPrefixId + "-Text");
                settings.referencesTbl = document.getElementById(referencesId); 
                settings.addReferenceBtn = document.getElementById(addReferenceBtn);
                settings.referenceReqTitle = document.getElementById(referenceReqId + "-Title");
                settings.referenceReqText = document.getElementById(referenceReqId + "-Text");
                AddEvents();
            }
        }
    }(jQuery);