/*
    Model is responsible for metadata inputs
    initialize interface inputs: elements from <div id="hybris-panel-metdata">
    create metadata.txt
*/
window.Rigsarkiv = window.Rigsarkiv || {},
    function (n) {
        Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
        function (n) {
            const { ipcRenderer } = require('electron');
            const fs = require('fs');
            const path = require('path');
            const os = require('os');
            const chardet = require('chardet');

            const startNumberPattern = /^([0-9])([a-zA-ZæøåÆØÅ0-9_]*)$/;
            const validFileNamePattern = /^([a-zA-ZæøåÆØÅ])([a-zA-ZæøåÆØÅ0-9_]*)$/;
            const reservedWordPattern = /^(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)$/i;
            const enclosedReservedWordPattern = /^(")(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)(")$/i;
            const strLength = 128;

            //private data memebers
            var settings = {
                fileName: null,
                fileDescr: null,
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
                referencesTab: null,
                indexfilesTab: null,
                informationPanel1: null,
                informationPanel2: null,
                indexFilesDescriptionSpn: null,
                indexFilesDescriptionText: null,
                variablesDropdown: null,
                cancelBtn: null,
                tablesDropdown: null,
                foreignTablesDropdown: null,
                refVarsDropdown: null,
                foreignVariablesDropdown: null,
                newExtractionWarningTitle: null,
                newExtractionWarningText: null,
                contents: ["","","",""],
                varKeys: [],
                references: [],
                variables: [],
                dataPathPostfix: "Data",
                metadataFileName: "{0}.txt",
                dataFileName: "{0}.csv",
                metadataTemplateFileName: "metadata.txt",
                scriptPath: "./assets/scripts/{0}",
                resourceWinPath: "resources\\{0}",
                styleBox: null
            }

            //reset status & input fields
            var Reset = function () {
                settings.informationPanel1.hidden = false;
                settings.informationPanel2.hidden = true;
                settings.outputErrorSpn.hidden = true;
                settings.nextBtn.hidden = true;
                settings.contents = ["","","",""];
                settings.styleBox.hidden = true;
            }        

            //get metadata file name
            var GetMetaDataFileName = function() {
                return settings.metadataFileName.format(GetDataFolderName());
            }

            // get selected data table folder name 
            var GetDataFolderName = function() {
                var dataFolderPath = Rigsarkiv.Hybris.DataExtraction.callback().dataFolderPath;
                var folders = dataFolderPath.getFolders();
                return folders[folders.length - 1];
            }

            var GetNewLine = function() {
                var result = "\r";
                if(os.platform() == "win32") { result = "\r\n"; }
                if(os.platform() == "darwin") { result = "\n"; }
                return result;
            }

            // Cleanup Files
            var CleanupFiles = function(files,fileName) {
                var result = false;
                var dataFolderPath = Rigsarkiv.Hybris.DataExtraction.callback().dataFolderPath;
                files.forEach(file => {
                    if(file != settings.dataFileName.format(fileName) && file != settings.metadataFileName.format(fileName)) {
                        console.logInfo("delete file : " + file,"Rigsarkiv.Hybris.MetaData.Cleanup");
                        try {
                            var srcFilePath = dataFolderPath;
                            srcFilePath += (srcFilePath.indexOf("\\") > -1) ? "\\{0}".format(file) : "/{0}".format(file);
                            fs.unlinkSync(srcFilePath);                                
                        }
                        catch(err) {
                            result = true;
                            err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.MetaData.CleanupFiles");
                        }                            
                    }
                });
                return result;
            }

            //delete other txt files from statistic program
            var Cleanup = function() {
                var dataFolderPath = Rigsarkiv.Hybris.DataExtraction.callback().dataFolderPath;
                fs.readdir(dataFolderPath, (err, files) => {
                    if (err) {
                        err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.MetaData.Cleanup");
                    }
                    else {
                        var fileName = GetDataFolderName();                        
                        var hasError = CleanupFiles(files,fileName);
                        var callback = Rigsarkiv.Hybris.DataExtraction.callback();
                        var dataFolderPath = callback.dataFolderPath;                        
                        if(hasError) {
                            ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-CloseApplicationError-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-CloseApplicationError-Text"));
                        }
                        else {
                            var folders = callback.selectedStatisticsFilePath.getFolders();                            
                            settings.outputOkSpn.innerHTML = settings.outputOkText.format(settings.dataFileName.format(fileName),settings.metadataFileName.format(fileName),folders[folders.length - 1]);
                            settings.okDataPath.innerHTML = callback.localFolderPath;
                            folders = dataFolderPath.getFolders();
                            settings.outputNewExtractionSpn.innerHTML = settings.outputNewExtractionText.format(folders[folders.length - 3]);
                            settings.indexFilesDescriptionSpn.innerHTML = settings.indexFilesDescriptionText.format(folders[folders.length - 3]);
                            EnsureVariables();
                            Rigsarkiv.Hybris.Base.callback().metadata.push({ "fileName":fileName, "name":settings.fileName.value, "variables":settings.variables, "keys":settings.varKeys, "references":[] });
                            console.log("{0} data output: ".format(settings.fileName));
                            console.log(Rigsarkiv.Hybris.Base.callback().metadata);
                            settings.informationPanel1.hidden = true;
                            settings.informationPanel2.hidden = false;
                            settings.nextBtn.hidden = false;
                        }                                                          
                    }
                });
            }

            //rename Statistics file name 
            var RenameFile = function() {
                var folders = Rigsarkiv.Hybris.DataExtraction.callback().selectedStatisticsFilePath.getFolders();
                var srcFileName = folders[folders.length - 1];
                srcFileName = srcFileName.substring(0,srcFileName.indexOf("."));
                srcFileName = settings.dataFileName.format(srcFileName);
                var destFileName = GetDataFolderName();
                destFileName = settings.dataFileName.format(destFileName);
                var dataFolderPath = Rigsarkiv.Hybris.DataExtraction.callback().dataFolderPath;

                console.logInfo("rename {0} file to: {1}".format(srcFileName,destFileName),"Rigsarkiv.Hybris.MetaData.RenameFile");                
                var srcFilePath = dataFolderPath;
                srcFilePath += (srcFilePath.indexOf("\\") > -1) ? "\\{0}".format(srcFileName) : "/{0}".format(srcFileName);
                var destFilePath = dataFolderPath;
                destFilePath += (destFilePath.indexOf("\\") > -1) ? "\\{0}".format(destFileName) : "/{0}".format(destFileName);
                fs.rename(srcFilePath,destFilePath, (err) => {
                    if (err) {
                        err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.MetaData.RenameFile");
                    }
                    else {
                        console.logInfo("Rename complete!","Rigsarkiv.Hybris.MetaData.RenameFile");
                        Cleanup();
                    }
                }); 
            }

            //update metadata txt file
            var UpdateFile = function() {
                var metadataFileName = GetMetaDataFileName();
                var srcFilePath = Rigsarkiv.Hybris.DataExtraction.callback().dataFolderPath;
                srcFilePath += (srcFilePath.indexOf("\\") > -1) ? "\\{0}".format(metadataFileName) : "/{0}".format(metadataFileName);
                fs.readFile(srcFilePath, (err, data) => {
                    if (err) {
                        err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.MetaData.UpdateFile");
                    }
                    else {
                        var callback = Rigsarkiv.Hybris.DataExtraction.callback();
                        var metadataFileName = GetMetaDataFileName();
                        var scriptType = callback.scriptType;
                        var keyVar = (settings.varKeys.length > 0) ? "{0}{1}".format(settings.varKeys.join(" "),GetNewLine()) : "";
                        var updatedData = data.toString().format(scriptType,settings.fileName.value,settings.fileDescr.value,keyVar,"{0}",settings.contents[0],settings.contents[1],settings.contents[2],settings.contents[3]);
                        var srcFilePath = callback.dataFolderPath;
                        srcFilePath += (srcFilePath.indexOf("\\") > -1) ? "\\{0}".format(metadataFileName) : "/{0}".format(metadataFileName);
                        fs.writeFile(srcFilePath, updatedData, (err) => {
                            if (err) {
                                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.MetaData.UpdateFile");
                            }
                            else {
                                RenameFile();
                            }
                        });
                    }
                });
            }

            //copy metadata txt file to data table
            var EnsureFile = function() {
                var dataFolderPath = Rigsarkiv.Hybris.DataExtraction.callback().dataFolderPath;
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
                        rootPath = folders.slice(0,folders.length - 4).join("/");
                        metadataFilePath = "{0}/{1}".format(rootPath,settings.metadataTemplateFileName);
                    }
                }

                console.logInfo(`copy ${settings.metadataTemplateFileName} file to: ${dataFolderPath}`,"Rigsarkiv.Hybris.MetaData.EnsureFile");
                var destFilePath = dataFolderPath;
                destFilePath += (destFilePath.indexOf("\\") > -1) ? "\\{0}".format(metadataFileName) : "/{0}".format(metadataFileName);
                fs.copyFile(metadataFilePath,destFilePath, (err) => {
                    if (err) {
                        err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.MetaData.EnsureFile");
                    }
                    else {
                        console.logInfo("{0} was copied to {1}".format(GetMetaDataFileName(),Rigsarkiv.Hybris.DataExtraction.callback().dataFolderPath),"Rigsarkiv.Hybris.MetaData.EnsureFile");
                        UpdateFile();
                    }
                });
            }

            //remove UTF8 boom charactors
            var GetFileContent = function(filePath) {
                var fileContent = fs.readFileSync(filePath);
                if(fileContent.byteLength >= 3 && (fileContent[0] & 0xff) == 0xef && (fileContent[1] & 0xff) == 0xbb && (fileContent[2] & 0xff) == 0xbf) {
                    return fileContent.toString("utf8",3);
                }
                else {
                    return fileContent.toString();
                }
            }

            //build metadata file content
            var EnsureData = function() {
                var dataFolderPath = Rigsarkiv.Hybris.DataExtraction.callback().dataFolderPath;
                fs.readdir(dataFolderPath, (err, files) => {
                    if (err) {
                        err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.MetaData.EnsureData");
                    }
                    else {
                        var dataFolderPath = Rigsarkiv.Hybris.DataExtraction.callback().dataFolderPath;
                        console.logInfo(`get texts contents: ${dataFolderPath}`,"Rigsarkiv.Hybris.MetaData.EnsureData");
                        files.forEach(file => {
                            var filePath = dataFolderPath;
                            filePath += (filePath.indexOf("\\") > -1) ? "\\{0}".format(file) : "/{0}".format(file);
                            if(file.lastIndexOf("_VARIABEL.txt") > -1) {
                                settings.contents[0] = GetFileContent(filePath);
                            }
                            if(file.lastIndexOf("_VARIABELBESKRIVELSE.txt") > -1) {
                                settings.contents[1] = GetFileContent(filePath);
                            }
                            if(file.lastIndexOf("_KODELISTE.txt") > -1) {
                                settings.contents[2] = GetFileContent(filePath);
                            }
                            if(file.lastIndexOf("_BRUGERKODE.txt") > -1) {
                                settings.contents[3] = GetFileContent(filePath);
                            }
                        });
                        EnsureFile();
                    }
                });
            }

            //validation of input fileds with dialog popups
            var ValidateFields = function() {
                var result = true;
                if (settings.fileName.value === "") {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileNameRequired-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileNameRequired-Text"));
                    result = false;
                }
                if(result && settings.fileDescr.value === "") {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileDescrRequired-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileDescrRequired-Text"));
                    result = false;
                }
                if (result && startNumberPattern.test(settings.fileName.value)) {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileNameNumberFirst-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileNameNumberFirst-Text"));
                    result = false;
                }
                if (result && !validFileNamePattern.test(settings.fileName.value)) {
                    if(!enclosedReservedWordPattern.test(settings.fileName.value)) {
                        ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileNameIllegalChar-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileNameIllegalChar-Text"));
                        result = false;
                    }
                }
                if (result && settings.fileName.value.length > strLength) {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileNameLength-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileNameLength-Text"));
                    result = false;
                }
                if (result && reservedWordPattern.test(settings.fileName.value)) {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileNamereservedWord-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-fileNamereservedWord-Text"));
                    result = false;
                }
                return result;
            }

            // validate keys
            var ValidateKey = function(keyValue) {
                var result = true;
                if (result && startNumberPattern.test(keyValue)) {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-keyNumberFirst-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-keyNumberFirst-Text"));
                    result = false;
                }
                if (result && !validFileNamePattern.test(keyValue)) {
                    if(!enclosedReservedWordPattern.test(keyValue)) {
                        ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-keyIllegalChar-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-keyIllegalChar-Text"));
                        result = false;
                    }
                }
                if (result && keyValue.length > strLength) {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-keyLength-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-keyLength-Text"));
                    result = false;
                }
                if (result && reservedWordPattern.test(keyValue)) {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-keyReservedWord-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-keyReservedWord-Text"));
                    result = false;
                }
                return result;
            }

            //implments new data table extraction
            var ResetExtraction = function() {
                Rigsarkiv.Hybris.DataExtraction.callback().reset();
                Reset();
                $("#{0} tr:not(:first-child)".format(settings.varKeysTbl.id)).remove();
                settings.varKeysTbl.hidden = true;
                settings.varKeys = [];
                settings.variables = [];
                settings.fileName.value = "";
                settings.fileDescr.value = "";
            }

            var EnsureVariables = function() {
                if(settings.variables.length === 0) {
                    for (var i = 1; i < settings.variablesDropdown.options.length; i++) {
                        settings.variables.push(settings.variablesDropdown.options[i].value);
                    }
                }
            }

            //create HTML select option element
            var CreateOption = function(value,text) {
                var result = document.createElement('option');
                result.textContent = text;
                result.value = value;
                return result;
            }

             //Redirect to references or index files
             var Redirect = function() {
                var tablesCounter = 0;
                $(settings.tablesDropdown).empty();
                $(settings.foreignTablesDropdown).empty();
                settings.tablesDropdown.appendChild(CreateOption("",""));
                settings.foreignTablesDropdown.appendChild(CreateOption("",""));
                Rigsarkiv.Hybris.Base.callback().metadata.forEach(table => {
                    tablesCounter = tablesCounter + 1;
                    settings.tablesDropdown.appendChild(CreateOption(table.name, "{0} ({1})".format(table.name,table.fileName)));
                    settings.foreignTablesDropdown.appendChild(CreateOption(table.name, "{0} ({1})".format(table.name,table.fileName)));
                });
                if(tablesCounter === 1 && Rigsarkiv.Hybris.References.callback().updateFile(Rigsarkiv.Hybris.Base.callback().metadata[0])) {
                    settings.indexfilesTab.click();
                } 
                else {
                    settings.referencesTab.click(); 
                }
             }

            //add Event Listener to HTML elmenets
            var AddEvents = function () {
                settings.nextBtn.addEventListener('click', (event) => {
                    Redirect();
                });
                settings.newExtractionBtn.addEventListener('click', (event) => {
                    ipcRenderer.send('open-confirm-dialog','metadata-newextraction',settings.newExtractionWarningTitle.innerHTML,settings.newExtractionWarningText.innerHTML,Rigsarkiv.Language.callback().getValue("hybris-output-metdata-OkConfirm"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-CancelConfirm"));
                });
                settings.okDataPath.addEventListener('click', (event) => {
                    ipcRenderer.send('open-item',Rigsarkiv.Hybris.DataExtraction.callback().localFolderPath);
                })
                settings.okBtn.addEventListener('click', function (event) {
                    Reset();
                    if(ValidateFields()) 
                    { 
                        if(settings.variablesDropdown.selectedIndex > 0) {
                            ipcRenderer.send('open-confirm-dialog','metadata-addkey',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-addKeyWarning-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-addKeyWarning-Text"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-OkConfirm"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-CancelConfirm"));
                        } 
                        else {
                            EnsureData();
                        }                       
                    }
                })
                settings.cancelBtn.addEventListener('click', function (event) {
                    $("#{0} tr:not(:first-child)".format(settings.varKeysTbl.id)).remove();
                    settings.varKeysTbl.hidden = true;
                    settings.varKeys = [];
                    settings.variablesDropdown.selectedIndex = 0;    
                })
                settings.addVarKeyBtn.addEventListener('click', function (event) {
                    EnsureVariables();                           
                    var key = settings.variablesDropdown.options[settings.variablesDropdown.selectedIndex].value;
                    if(key != null && key !== "") {
                        if(ValidateKey(key) && !settings.varKeys.includes(key) && settings.variables.includes(key)) {
                            settings.varKeys.push(key);
                            settings.varKeysTbl.hidden = false;
                            $(settings.varKeysTbl).append("<tr><td>{0}</td></tr>".format(key));
                            settings.variablesDropdown.selectedIndex = 0;
                        }
                    }
                    else {
                        ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-metdata-varKeyRequired-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-metdata-varKeyRequired-Text"));
                    }
                });
                ipcRenderer.on('confirm-dialog-selection-metadata-addkey', (event, index) => {
                    if(index === 0) {
                        EnsureData();
                    } 
                    if(index === 1) { } 
                });
                ipcRenderer.on('confirm-dialog-selection-metadata-newextraction', (event, index) => {
                    if(index === 0) {
                        ResetExtraction();
                        settings.extractionTab.click();
                    } 
                    if(index === 1) { } 
                });
            }

            //Model interfaces functions
            Rigsarkiv.Hybris.MetaData = {
                initialize: function (metadataFileName,metadataFileNameDescription,metdataOkBtn,outputOkId,okDataPathId,outputErrorId,outputNewExtractionId,newExtractionBtn,extractionTabId,outputNextId,nextBtn,referencesTabId,informationPanel1Id,informationPanel2Id,indexFilesDescriptionId,resetHideBox,variablesId,addVarKeyId,varKeysId,tablesId,foreignTablesId,cancelId,indexfilesTabId,refVarsId,foreignVariablesId,newExtractionWarningId) {
                    settings.fileName = document.getElementById(metadataFileName);
                    settings.fileDescr = document.getElementById(metadataFileNameDescription);
                    settings.okBtn = document.getElementById(metdataOkBtn);
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
                    settings.referencesTab = document.getElementById(referencesTabId);
                    settings.informationPanel1 = document.getElementById(informationPanel1Id);
                    settings.informationPanel2 = document.getElementById(informationPanel2Id);
                    settings.indexFilesDescriptionSpn = document.getElementById(indexFilesDescriptionId);
                    settings.indexFilesDescriptionText = settings.indexFilesDescriptionSpn.innerHTML;
                    settings.styleBox = document.getElementById(resetHideBox);
                    settings.variablesDropdown = document.getElementById(variablesId);
                    settings.addVarKeyBtn = document.getElementById(addVarKeyId);
                    settings.varKeysTbl = document.getElementById(varKeysId);
                    settings.tablesDropdown = document.getElementById(tablesId);
                    settings.foreignTablesDropdown = document.getElementById(foreignTablesId);
                    settings.cancelBtn = document.getElementById(cancelId);
                    settings.indexfilesTab = document.getElementById(indexfilesTabId);
                    settings.refVarsDropdown = document.getElementById(refVarsId);
                    settings.foreignVariablesDropdown = document.getElementById(foreignVariablesId);
                    settings.newExtractionWarningTitle = document.getElementById(newExtractionWarningId + "-Title");
                    settings.newExtractionWarningText = document.getElementById(newExtractionWarningId + "-Text");
                    AddEvents();
                },
                callback: function () {
                    return { 
                        reset: function() 
                        { 
                            Rigsarkiv.Hybris.Base.callback().setMetadata([]);
                            Reset();
                            ResetExtraction();
                        } 
                    };
                }
            }
        }(jQuery);
    }(jQuery);