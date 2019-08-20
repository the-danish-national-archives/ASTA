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
                metadataCallback: null,
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
                settings.metadataCallback().data.forEach(table => {
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
            }

            //Update metadata file references
            var UpdateFile = function(filePath,references) {
                console.logInfo("update metadata file: {0}".format(filePath),"Rigsarkiv.Hybris.References.UpdateFile");
                fs.readFile(filePath, (err, data) => {
                    if (err) {
                        err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.References.UpdateFile");
                    }
                    else {
                        var text = "";
                        if(references.length > 0) {
                            references.forEach(reference => {
                                text += "{0} {1} {2}{3}".format(reference.table,"'{0}'".format(reference.key),"'{0}'".format(reference.refKey),GetNewLine());
                            });
                        }
                        var updatedData = data.toString().format(text);
                        fs.writeFile(filePath, updatedData, (err) => {
                            if (err) {
                                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.References.UpdateFile");
                            }
                            else {
                                
                            }
                        });
                    }
                });
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
                    var dataFolderPath = settings.metadataCallback().structureCallback.deliveryPackagePath;
                    dataFolderPath = (dataFolderPath.indexOf("\\") > -1) ? "{0}\\{1}".format(dataFolderPath,settings.dataPathPostfix) : "{0}/{1}".format(dataFolderPath,settings.dataPathPostfix);
                    settings.metadataCallback().data.forEach(table => {
                        var fileName = settings.metadataFileName.format(table.fileName);
                        var path = (dataFolderPath.indexOf("\\") > -1) ? "{0}\\{1}\\{2}".format(dataFolderPath,table.fileName,fileName) : "{0}/{1}/{2}".format(dataFolderPath,table.fileName,fileName);
                        UpdateFile(path,table.references);
                    });
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
                    if (settings.tableBox.value !== "" && settings.foreignTableBox.value !== "" && settings.refVarBox.value !== "" && settings.foreignVariableBox.value !== "") {
                        if(ValidateReference("foreignVariable",settings.foreignVariableBox.value) && ValidateReference("referenceVariable",settings.refVarBox.value)) {
                            var table = GetTableData(settings.tableBox.value);
                            table.references.push({"table":settings.foreignTableBox.value, "key":settings.foreignVariableBox.value, "refKey":settings.refVarBox.value});
                            $(settings.referencesTbl).append("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>".format(settings.tableBox.value,settings.refVarBox.value,settings.foreignTableBox.value,settings.foreignVariableBox.value));
                            settings.referencesTbl.hidden = false;
                            settings.tableBox.value = "";
                            settings.foreignTableBox.value = "";
                            settings.refVarBox.value = "";
                            settings.foreignVariableBox.value = "";
                        }
                    }
                    else {
                        ipcRenderer.send('open-error-dialog',settings.referenceReqTitle.innerHTML,settings.referenceReqText.innerHTML);
                    }               
                });    
            }

            //Model interfaces functions
            Rigsarkiv.Hybris.References = { 
                initialize: function (metadataCallback,outputErrorId,okId,tablesId,tableBoxId,refVarsId,foreignTableBoxId,foreignVariablesId,addReferenceBtn,referenceReqId,refVarBoxId,foreignVariableBoxId,numberFirstReference,illegalCharReference,referenceLength,referenceReservedWord,foreignVariableId,referenceVariableId,referencesId) {
                    settings.metadataCallback = metadataCallback;
                    settings.outputErrorSpn = document.getElementById(outputErrorId);
                    settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                    settings.okBtn = document.getElementById(okId);
                    settings.tablesDropdown = document.getElementById(tablesId); 
                    settings.tableBox = document.getElementById(tableBoxId);
                    settings.refVarsDropdown = document.getElementById(refVarsId);
                    settings.foreignTableBox = document.getElementById(foreignTableBoxId);
                    settings.foreignVariablesDropdown = document.getElementById(foreignVariablesId);
                    settings.addReferenceBtn = document.getElementById(addReferenceBtn);
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
                    AddEvents();
                }
            }
        }(jQuery);
    }(jQuery);