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

            const startNumberPattern = /^([0-9])([a-zA-ZæøåÆØÅ0-9_]*)$/;
            const validFileNamePattern = /^([a-zA-ZæøåÆØÅ])([a-zA-ZæøåÆØÅ0-9_]*)$/;
            const reservedWordPattern = /^(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)$/i;
            const enclosedReservedWordPattern = /^(")(ABSOLUTE|ACTION|ADD|ADMIN|AFTER|AGGREGATE|ALIAS|ALL|ALLOCATE|ALTER|AND|ANY|ARE|ARRAY|AS|ASC|ASSERTION|AT|AUTHORIZATION|BEFORE|BEGIN|BINARY|BIT|BLOB|BOOLEAN|BOTH|BREADTH|BY|CALL|CASCADE|CASCADED|CASE|CAST|CATALOG|CHAR|CHARACTER|CHECK|CLASS|CLOB|CLOSE|COLLATE|COLLATION|COLUMN|COMMIT|COMPLETION|CONNECT|CONNECTION|CONSTRAINT|CONSTRAINTS ||CONSTRUCTOR|CONTINUE|CORRESPONDING|CREATE|CROSS|CUBE|CURRENT|CURRENT_DATE|CURRENT_PATH|CURRENT_ROLE|CURRENT_TIME|CURRENT_TIMESTAMP|CURRENT_USER|CURSOR|CYCLE|DATA|DATE|DAY|DEALLOCATE|DEC|DECIMAL|DECLARE|DEFAULT|DEFERRABLE|DEFERRED|DELETE|DEPTH|DEREF|DESC|DESCRIBE|DESCRIPTOR|DESTROY|DESTRUCTOR|DETERMINISTIC|DICTIONARY|DIAGNOSTICS|DISCONNECT|DISTINCT|DOMAIN|DOUBLE|DROP|DYNAMIC|EACH|ELSE|END|END-EXEC|EQUALS|ESCAPE|EVERY|EXCEPT|EXCEPTION|EXEC|EXECUTE|EXTERNAL|FALSE|FETCH|FIRST|FLOAT|FOR|FOREIGN|FOUND|FROM|FREE|FULL|FUNCTION|GENERAL|GET|GLOBAL|GO|GOTO|GRANT|GROUP|GROUPING|HAVING|HOST|HOUR|IDENTITY|IGNORE|IMMEDIATE|IN|INDICATOR|INITIALIZE|INITIALLY|INNER|INOUT|INPUT|INSERT|INT|INTEGER|INTERSECT|INTERVAL|INTO|IS|ISOLATION|ITERATE|JOIN|KEY|LANGUAGE|LARGE|LAST|LATERAL|LEADING|LEFT|LESS|LEVEL|LIKE|LIMIT|LOCAL|LOCALTIME|LOCALTIMESTAMP|LOCATOR|MAP|MATCH|MINUTE|MODIFIES|MODIFY|MODULE|MONTH|NAMES|NATIONAL|NATURAL|NCHAR|NCLOB|NEW|NEXT|NO|NONE|NOT|NULL|NUMERIC|OBJECT|OF|OFF|OLD|ON|ONLY|OPEN|OPERATION|OPTION|OR|ORDER|ORDINALITY|OUT|OUTER|OUTPUT|PAD|PARAMETER|PARAMETERS|PARTIAL|PATH|POSTFIX|PRECISION|PREFIX|PREORDER|PREPARE|PRESERVE|PRIMARY|PRIOR|PRIVILEGES|PROCEDURE|PUBLIC|READ|READS|REAL|RECURSIVE|REF|REFERENCES|REFERENCING|RELATIVE|RESTRICT|RESULT|RETURN|RETURNS|REVOKE|RIGHT|ROLE|ROLLBACK|ROLLUP|ROUTINE|ROW|ROWS|SAVEPOINT|SCHEMA|SCROLL|SCOPE|SEARCH|SECOND|SECTION|SELECT|SEQUENCE|SESSION|SESSION_USER|SET|SETS|SIZE|SMALLINT|SOME|SPACE|SPECIFIC|SPECIFICTYPE|SQL|SQLEXCEPTION|SQLSTATE|SQLWARNING|START|STATE|STATEMENT|STATIC|STRUCTURE|SYSTEM_USER|TABLE|TEMPORARY|TERMINATE|THAN|THEN|TIME|TIMESTAMP|TIMEZONE_HOUR|TIMEZONE_MINUTE|TO|TRAILING|TRANSACTION|TRANSLATION|TREAT|TRIGGER|TRUE|UNDER|UNION|UNIQUE|UNKNOWN|UNNEST|UPDATE|USAGE|USER|USING|VALUE|VALUES|VARCHAR|VARIABLE|VARYING|VIEW|WHEN|WHENEVER|WHERE|WITH|WITHOUT|WORK|WRITE|YEAR|ZONE)(")$/i;
            const strLength = 128;

            //private data memebers
            var settings = {
                outputErrorSpn: null,
                outputErrorText: null,
                okBtn: null,
                tablesDropdown: null,
                tableBox: null,
                refVarsDropdown: null,
                foreignTableBox: null,
                foreignVariablesDropdown: null,
                addReferenceBtn: null,
                referenceReqTitle: null,
                referenceReqText: null,
                refVarBox: null,
                foreignVariableBox: null,
                numberFirstReferenceText: null,
                illegalCharReferenceText: null,
                referenceLengthText: null,
                referenceReservedWordText: null,
                foreignVariableTitle: null,
                referenceVariableTitle: null,
                referencesTbl: null,
                indexfilesTab: null,
                addReferenceVariableBtn: null,
                referenceVariablesTbl: null,
                addForeignVariableBtn: null,
                foreignVariablesTbl: null,
                cancelBtn: null,
                addKeyWarningTitle: null,
                addKeyWarningText: null,
                okConfirm: null,
                cancelConfirm: null,
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
                settings.tableBox.value = "";
                settings.foreignTableBox.value = "";
                settings.refVarBox.value = "";
                settings.foreignVariableBox.value = "";
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
                    case "foreignVariable" : referenceTypeTitle = settings.foreignVariableTitle.innerHTML ;break;
                    case "referenceVariable" : referenceTypeTitle = settings.referenceVariableTitle.innerHTML ;break;
                }
                if (result && startNumberPattern.test(referenceValue)) {
                    ipcRenderer.send('open-error-dialog',referenceTypeTitle,settings.numberFirstReferenceText.innerHTML.format(referenceTypeTitle));
                    result = false;
                }
                if (result && !validFileNamePattern.test(referenceValue)) {
                    if(!enclosedReservedWordPattern.test(referenceValue)) {
                        ipcRenderer.send('open-error-dialog',referenceTypeTitle,settings.illegalCharReferenceText.innerHTML.format(referenceTypeTitle));
                        result = false;
                    }
                }
                if (result && referenceValue.length > strLength) {
                    ipcRenderer.send('open-error-dialog',referenceTypeTitle,settings.referenceLengthText.innerHTML.format(referenceTypeTitle));
                    result = false;
                }
                if (result && reservedWordPattern.test(referenceValue)) {
                    ipcRenderer.send('open-error-dialog',referenceTypeTitle,settings.referenceReservedWordText.innerHTML.format(referenceTypeTitle));
                    result = false;
                }
                return result;
            }

            //add Event Listener to HTML elmenets
            var AddEvents = function () {
                settings.okBtn.addEventListener('click', function (event) {    
                    if(settings.foreignVariableBox.value !== "" || settings.refVarBox.value !== "" || settings.referenceVariables.length > 0 || settings.foreignVariables > 0) {
                        ipcRenderer.send('open-confirm-dialog','references-addkey',settings.addKeyWarningTitle.innerHTML,settings.addKeyWarningText.innerHTML,settings.okConfirm.innerHTML,settings.cancelConfirm.innerHTML);
                    }
                    else {
                        Redirect();
                    }
                });
                settings.cancelBtn.addEventListener('click', function (event) {                    
                    Reset();                    
                });
                settings.tableBox.addEventListener('change', function (event) {
                    $(settings.refVarsDropdown).empty();
                    settings.refVarBox.value = "";
                    var table = GetTableData(event.srcElement.value);
                    table.variables.forEach(variable => {
                        var el = document.createElement('option');
                        el.textContent = variable;
                        el.value = variable;
                        settings.refVarsDropdown.appendChild(el);
                    });                    
                });
                settings.foreignTableBox.addEventListener('change', function (event) {
                    $(settings.foreignVariablesDropdown).empty();
                    settings.foreignVariableBox.value = "";
                    var table = GetTableData(event.srcElement.value);
                    table.variables.forEach(variable => {
                        var el = document.createElement('option');
                        el.textContent = variable;
                        el.value = variable;
                        settings.foreignVariablesDropdown.appendChild(el);
                    });                    
                });
                settings.addReferenceBtn.addEventListener('click', function (event) {
                    if (settings.tableBox.value !== "" && settings.foreignTableBox.value !== "" && settings.referenceVariables.length > 0 && settings.foreignVariables.length > 0) {
                            var table = GetTableData(settings.tableBox.value);
                            table.references.push({"table":settings.foreignTableBox.value, "key":settings.foreignVariables, "refKey":settings.referenceVariables});
                            $(settings.referencesTbl).append("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>".format(settings.tableBox.value,settings.referenceVariables.join(" "),settings.foreignTableBox.value,settings.foreignVariables.join(" ")));
                            settings.referencesTbl.hidden = false;
                            settings.tableBox.value = "";
                            settings.foreignTableBox.value = "";
                            Clear();
                    }
                    else {
                        ipcRenderer.send('open-error-dialog',settings.referenceReqTitle.innerHTML,settings.referenceReqText.innerHTML);
                    }               
                });
                settings.addReferenceVariableBtn.addEventListener('click', function (event) { 
                    if(settings.refVarBox.value !== "" && ValidateReference("referenceVariable",settings.refVarBox.value) && !settings.referenceVariables.includes(settings.refVarBox.value)) {
                        settings.referenceVariables.push(settings.refVarBox.value);
                        $(settings.referenceVariablesTbl).append("<tr><td>{0}</td></tr>".format(settings.refVarBox.value));
                        settings.referenceVariablesTbl.hidden = false;
                        settings.refVarBox.value = "";
                    }
                });
                settings.addForeignVariableBtn.addEventListener('click', function (event) { 
                    if(settings.foreignVariableBox.value !== "" && ValidateReference("foreignVariable",settings.foreignVariableBox.value) && !settings.foreignVariables.includes(settings.foreignVariableBox.value)) {
                        settings.foreignVariables.push(settings.foreignVariableBox.value);
                        $(settings.foreignVariablesTbl).append("<tr><td>{0}</td></tr>".format(settings.foreignVariableBox.value));
                        settings.foreignVariablesTbl.hidden = false;
                        settings.foreignVariableBox.value = "";
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
                initialize: function (outputErrorId,okId,tablesId,tableBoxId,refVarsId,foreignTableBoxId,foreignVariablesId,addReferenceId,referenceReqId,refVarBoxId,foreignVariableBoxId,numberFirstReference,illegalCharReference,referenceLength,referenceReservedWord,foreignVariableId,referenceVariableId,referencesId,indexfilesTabId,addReferenceVariableId,referenceVariablesId,addForeignVariableId,foreignVariablesTable,cancelId,addKeyWarningId,okConfirmId,cancelConfirmId) {
                    settings.outputErrorSpn = document.getElementById(outputErrorId);
                    settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                    settings.okBtn = document.getElementById(okId);
                    settings.tablesDropdown = document.getElementById(tablesId); 
                    settings.tableBox = document.getElementById(tableBoxId);
                    settings.refVarsDropdown = document.getElementById(refVarsId);
                    settings.foreignTableBox = document.getElementById(foreignTableBoxId);
                    settings.foreignVariablesDropdown = document.getElementById(foreignVariablesId);
                    settings.addReferenceBtn = document.getElementById(addReferenceId);
                    settings.referenceReqTitle = document.getElementById(referenceReqId + "-Title");
                    settings.referenceReqText = document.getElementById(referenceReqId + "-Text");
                    settings.refVarBox = document.getElementById(refVarBoxId);
                    settings.foreignVariableBox = document.getElementById(foreignVariableBoxId);
                    settings.numberFirstReferenceText = document.getElementById(numberFirstReference + "-Text");
                    settings.illegalCharReferenceText = document.getElementById(illegalCharReference + "-Text");
                    settings.referenceLengthText = document.getElementById(referenceLength + "-Text");
                    settings.referenceReservedWordText = document.getElementById(referenceReservedWord + "-Text");
                    settings.foreignVariableTitle = document.getElementById(foreignVariableId + "-Title");
                    settings.referenceVariableTitle = document.getElementById(referenceVariableId + "-Title");
                    settings.referencesTbl = document.getElementById(referencesId); 
                    settings.indexfilesTab = document.getElementById(indexfilesTabId);
                    settings.addReferenceVariableBtn = document.getElementById(addReferenceVariableId);
                    settings.referenceVariablesTbl = document.getElementById(referenceVariablesId);
                    settings.addForeignVariableBtn = document.getElementById(addForeignVariableId);
                    settings.foreignVariablesTbl = document.getElementById(foreignVariablesTable);
                    settings.cancelBtn = document.getElementById(cancelId);
                    settings.addKeyWarningTitle = document.getElementById(addKeyWarningId + "-Title");
                    settings.addKeyWarningText = document.getElementById(addKeyWarningId + "-Text");
                    settings.okConfirm = document.getElementById(okConfirmId);
                    settings.cancelConfirm = document.getElementById(cancelConfirmId);
                    AddEvents();
                },
                callback: function () {
                    return { 
                        reset: function() 
                        { 
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