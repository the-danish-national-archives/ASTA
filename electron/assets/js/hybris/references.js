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

            //private data memebers
            var settings = {
                metadataCallback: null,
                outputErrorSpn: null,
                outputErrorText: null,
                okBtn: null,
                tablesDropdown: null,
                tableBox: null,
                refVarsDropdown: null,
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
                    var table = GetTableData(event.srcElement.value);
                    table.variables.forEach(variable => {
                        var el = document.createElement('option');
                        el.textContent = variable;
                        el.value = variable;
                        settings.refVarsDropdown.appendChild(el);
                    });                    
                });
            }

            //Model interfaces functions
            Rigsarkiv.Hybris.References = { 
                initialize: function (metadataCallback,outputErrorId,okId,tablesId,tableBoxId,refVarsId) {
                    settings.metadataCallback = metadataCallback;
                    settings.outputErrorSpn = document.getElementById(outputErrorId);
                    settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                    settings.okBtn = document.getElementById(okId);
                    settings.tablesDropdown = document.getElementById(tablesId); 
                    settings.tableBox = document.getElementById(tableBoxId);
                    settings.refVarsDropdown = document.getElementById(refVarsId);
                    AddEvents();
                }
            }
        }(jQuery);
    }(jQuery);