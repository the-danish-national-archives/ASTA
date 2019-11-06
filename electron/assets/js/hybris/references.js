/*
    Model is responsible for references inputs
    initialize interface inputs: elements from <div id="hybris-panel-references">
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

            const startNumberPattern = /^([0-9])([a-zA-ZæøåäöáðéíóúýþÆØÅÄÖÁÐÉÍÓÚÝÞ0-9_]*)$/;
            const validFileNamePattern = /^([a-zA-ZæøåäöáðéíóúýþÆØÅÄÖÁÐÉÍÓÚÝÞ])([a-zA-ZæøåäöáðéíóúýþÆØÅÄÖÁÐÉÍÓÚÝÞ0-9_]*)$/;
            const reservedWordPattern = /^(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)$/i;
            const enclosedReservedWordPattern = /^(")(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)(")$/i;
            const strLength = 128;

            //private data memebers
            var settings = {
                outputErrorSpn: null,
                outputErrorText: null,
                okBtn: null,
                tablesDropdown: null,
                foreignTablesDropdown: null,
                refVarsDropdown: null,
                foreignVariablesDropdown: null,
                addReferenceBtn: null,
                referencesTbl: null,
                indexfilesTab: null,
                addReferenceVariableBtn: null,
                referenceVariablesTbl: null,
                addForeignVariableBtn: null,
                foreignVariablesTbl: null,
                cancelBtn: null,
                foreignVariables: [],
                referenceVariables: [],
                dataPathPostfix: "Data",
                metadataFileName: "{0}.txt"
            }

            var GetNewLine = function() {
                var result = "\r";
                if(os.platform() == "win32") { result = "\r\n"; }
                if(os.platform() == "darwin") { result = "\n"; }
                return result;
            }

            //get data JSON table object pointer by table file name
            var GetTableData = function (name) {
                var result = null;
                Rigsarkiv.Hybris.Base.callback().metadata.forEach(table => {
                    if(table.name === name) {
                        result = table;
                    }
                });
                return result;
            }

            //reset status & input fields
            var Reset = function () {
                $("#{0} tr:not(:first-child)".format(settings.referencesTbl.id)).remove();
                settings.referencesTbl.hidden = true;
                Clear();
                var metadata = Rigsarkiv.Hybris.Base.callback().metadata;
                if(metadata != null && metadata.length > 0) {
                    metadata.forEach(table => {
                        table.references = [];
                    });
                }               
            }

            // clear 1 reference row
            var Clear = function() {
                $("#{0} tr:not(:first-child)".format(settings.referenceVariablesTbl.id)).remove();
                $("#{0} tr:not(:first-child)".format(settings.foreignVariablesTbl.id)).remove();
                settings.referenceVariablesTbl.hidden = true;
                settings.foreignVariablesTbl.hidden = true;
                settings.referenceVariables = [];
                settings.foreignVariables = [];                
            } 

            //Update metadata file references
            var UpdateFile = function(dataFolderPath,table) {
                var result = true;
                try
                {
                    var fileName = settings.metadataFileName.format(table.fileName);
                    var filePath = (dataFolderPath.indexOf("\\") > -1) ? "{0}\\{1}\\{2}".format(dataFolderPath,table.fileName,fileName) : "{0}/{1}/{2}".format(dataFolderPath,table.fileName,fileName);
                    console.logInfo("update metadata file: {0}".format(filePath),"Rigsarkiv.Hybris.References.UpdateFile");
                    data = fs.readFileSync(filePath)
                    var text = "";
                    if(table.references.length > 0) {
                        table.references.forEach(reference => {
                            text += "{0} {1} {2}{3}".format(reference.table,"'{0}'".format(reference.key.join(" ")),"'{0}'".format(reference.refKey.join(" ")),GetNewLine());
                        });
                    }
                    var updatedData = data.toString().format(text);
                    fs.writeFileSync(filePath, updatedData);
                }
                catch(err) 
                {
                    err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.References.UpdateFile");
                }
                return result;               
            }

            //Redirect to index files
            var Redirect = function() {
                var result = true;
                var dataFolderPath = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath;
                dataFolderPath = (dataFolderPath.indexOf("\\") > -1) ? "{0}\\{1}".format(dataFolderPath,settings.dataPathPostfix) : "{0}/{1}".format(dataFolderPath,settings.dataPathPostfix);
                Rigsarkiv.Hybris.Base.callback().metadata.forEach(table => {
                    if(!UpdateFile(dataFolderPath,table)) { result = false; }
                });
                if(result) { settings.indexfilesTab.click(); }
            }

            // Validate refernces inputs
            var ValidateReference = function(referenceType,referenceValue) {
                var result = true;
                var referenceTypeTitle = "";
                switch(referenceType) {
                    case "foreignVariable" : referenceTypeTitle = Rigsarkiv.Language.callback().getValue("hybris-output-references-referenceForeignVariable-Title");break;
                    case "referenceVariable" : referenceTypeTitle = Rigsarkiv.Language.callback().getValue("hybris-output-references-referenceRefVar-Title");break;
                }
                if (result && startNumberPattern.test(referenceValue)) {
                    ipcRenderer.send('open-error-dialog',referenceTypeTitle,Rigsarkiv.Language.callback().getValue("hybris-output-references-referenceNumberFirst-Text").format(referenceTypeTitle));
                    result = false;
                }
                if (result && !validFileNamePattern.test(referenceValue)) {
                    if(!enclosedReservedWordPattern.test(referenceValue)) {
                        ipcRenderer.send('open-error-dialog',referenceTypeTitle,Rigsarkiv.Language.callback().getValue("hybris-output-references-referenceIllegalChar-Text").format(referenceTypeTitle));
                        result = false;
                    }
                }
                if (result && referenceValue.length > strLength) {
                    ipcRenderer.send('open-error-dialog',referenceTypeTitle,Rigsarkiv.Language.callback().getValue("hybris-output-references-referenceLength-Text").format(referenceTypeTitle));
                    result = false;
                }
                if (result && reservedWordPattern.test(referenceValue)) {
                    ipcRenderer.send('open-error-dialog',referenceTypeTitle,Rigsarkiv.Language.callback().getValue("hybris-output-references-referenceReservedWord-Text").format(referenceTypeTitle));
                    result = false;
                }
                return result;
            }

             //create HTML select option element
             var CreateOption = function(value,text) {
                var result = document.createElement('option');
                result.textContent = text;
                result.value = value;
                return result;
            }

            //add Event Listener to HTML elmenets
            var AddEvents = function () {
                settings.okBtn.addEventListener('click', function (event) {    
                    if(settings.foreignVariablesDropdown.selectedIndex > 0 || settings.refVarsDropdown.selectedIndex > 0 || settings.referenceVariables.length > 0 || settings.foreignVariables > 0) {
                        ipcRenderer.send('open-confirm-dialog','references-addkey',Rigsarkiv.Language.callback().getValue("hybris-output-references-addKeyWarning-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-references-addKeyWarning-Text"),Rigsarkiv.Language.callback().getValue("hybris-output-references-OkConfirm"),Rigsarkiv.Language.callback().getValue("hybris-output-references-CancelConfirm"));
                    }
                    else {
                        Redirect();
                    }
                });
                settings.cancelBtn.addEventListener('click', function (event) {                    
                    Reset();
                    settings.refVarsDropdown.selectedIndex = 0;
                    settings.foreignVariablesDropdown.selectedIndex = 0;
                    settings.tablesDropdown.selectedIndex = 0;
                    settings.foreignTablesDropdown.selectedIndex = 0;                    
                });
                settings.tablesDropdown.addEventListener('change', function (event) {
                    $(settings.refVarsDropdown).empty();
                    if(event.srcElement.selectedIndex === 0) { return; }
                    settings.refVarsDropdown.appendChild(CreateOption("",""));
                    var table = GetTableData(event.srcElement.value);
                    table.variables.forEach(variable => {
                        settings.refVarsDropdown.appendChild(CreateOption(variable,variable));
                    });                    
                });
                settings.foreignTablesDropdown.addEventListener('change', function (event) {
                    $(settings.foreignVariablesDropdown).empty();
                    if(event.srcElement.selectedIndex === 0) { return; }
                    settings.foreignVariablesDropdown.appendChild(CreateOption("",""));
                    var table = GetTableData(event.srcElement.value);
                    table.variables.forEach(variable => {
                        settings.foreignVariablesDropdown.appendChild(CreateOption(variable,variable));
                    });                    
                });
                settings.addReferenceBtn.addEventListener('click', function (event) {
                    var tableName = settings.tablesDropdown.options[settings.tablesDropdown.selectedIndex].value;
                    var foreignTableName = settings.foreignTablesDropdown.options[settings.foreignTablesDropdown.selectedIndex].value;
                    if (settings.referenceVariables.length > 0 && settings.foreignVariables.length > 0) {
                            var table = GetTableData(tableName);
                            table.references.push({"table":foreignTableName, "key":settings.foreignVariables, "refKey":settings.referenceVariables});
                            $(settings.referencesTbl).append("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>".format(tableName,settings.referenceVariables.join(" "),foreignTableName,settings.foreignVariables.join(" ")));
                            settings.referencesTbl.hidden = false;
                            Clear();
                            settings.refVarsDropdown.selectedIndex = 0;
                            settings.foreignVariablesDropdown.selectedIndex = 0;
                            settings.tablesDropdown.selectedIndex = 0;
                            settings.foreignTablesDropdown.selectedIndex = 0;
                    }
                    else {
                        ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-references-referenceRequired-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-references-referenceRequired-Text"));
                    }               
                });
                settings.addReferenceVariableBtn.addEventListener('click', function (event) { 
                    var refVar = settings.refVarsDropdown.options[settings.refVarsDropdown.selectedIndex].value;
                    if(settings.refVarsDropdown.selectedIndex > 0 && ValidateReference("referenceVariable",refVar) && !settings.referenceVariables.includes(refVar)) {
                        settings.referenceVariables.push(refVar);
                        $(settings.referenceVariablesTbl).append("<tr><td>{0}</td></tr>".format(refVar));
                        settings.referenceVariablesTbl.hidden = false;                        
                    }
                });
                settings.addForeignVariableBtn.addEventListener('click', function (event) { 
                    var foreignVariable = settings.foreignVariablesDropdown.options[settings.foreignVariablesDropdown.selectedIndex].value;
                    if(settings.foreignVariablesDropdown.selectedIndex > 0 && ValidateReference("foreignVariable",foreignVariable) && !settings.foreignVariables.includes(foreignVariable)) {
                        settings.foreignVariables.push(foreignVariable);
                        $(settings.foreignVariablesTbl).append("<tr><td>{0}</td></tr>".format(foreignVariable));
                        settings.foreignVariablesTbl.hidden = false;                        
                    }
                });
                ipcRenderer.on('confirm-dialog-selection-references-addkey', (event, index) => {
                    if(index === 0) {
                        Redirect();
                    } 
                    if(index === 1) { }            
                });   
            }

            //Model interfaces functions
            Rigsarkiv.Hybris.References = { 
                initialize: function (outputErrorId,okId,tablesId,foreignTablesId,refVarsId,foreignVariablesId,addReferenceId,referencesId,indexfilesTabId,addReferenceVariableId,referenceVariablesId,addForeignVariableId,foreignVariablesTable,cancelId) {
                    settings.outputErrorSpn = document.getElementById(outputErrorId);
                    settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                    settings.okBtn = document.getElementById(okId);
                    settings.tablesDropdown = document.getElementById(tablesId);
                    settings.foreignTablesDropdown = document.getElementById(foreignTablesId); 
                    settings.refVarsDropdown = document.getElementById(refVarsId);
                    settings.foreignVariablesDropdown = document.getElementById(foreignVariablesId);
                    settings.addReferenceBtn = document.getElementById(addReferenceId);
                    settings.referencesTbl = document.getElementById(referencesId); 
                    settings.indexfilesTab = document.getElementById(indexfilesTabId);
                    settings.addReferenceVariableBtn = document.getElementById(addReferenceVariableId);
                    settings.referenceVariablesTbl = document.getElementById(referenceVariablesId);
                    settings.addForeignVariableBtn = document.getElementById(addForeignVariableId);
                    settings.foreignVariablesTbl = document.getElementById(foreignVariablesTable);
                    settings.cancelBtn = document.getElementById(cancelId);
                    AddEvents();
                },
                callback: function () {
                    return { 
                        reset: function() 
                        { 
                            $(settings.tablesDropdown).empty();
                            $(settings.foreignTablesDropdown).empty();
                            $(settings.refVarsDropdown).empty();
                            $(settings.foreignVariablesDropdown).empty();                
                            Reset();
                        },
                        updateFile: function(table)
                        {
                            var dataFolderPath = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath;
                            dataFolderPath = (dataFolderPath.indexOf("\\") > -1) ? "{0}\\{1}".format(dataFolderPath,settings.dataPathPostfix) : "{0}/{1}".format(dataFolderPath,settings.dataPathPostfix);
                            return UpdateFile(dataFolderPath,table);
                        } 
                    };
                }
            }
        }(jQuery);
    }(jQuery);