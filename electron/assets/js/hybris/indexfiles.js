/*
    Model is responsible for upload index files
    initialize interface inputs: elements from <div id="hybris-panel-indexfiles">
    output 2 xml files at /Indices
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
    function (n) {
        const { ipcRenderer } = require('electron');
        const fs = require('fs');

        //private data memebers
        var settings = { 
            selectArchiveIndexFileBtn: null,
            pathArchiveIndexFileTxt: null,
            selectedArchiveIndexFilePath: null,
            selectContextDocumentationIndexFileBtn: null,
            pathContextDocumentationIndexFileTxt: null,
            selectedContextDocumentationIndexFilePath: null,
            okBtn: null,
            outputErrorSpn: null,
            outputErrorText: null,
            outputRequiredPathTitle: null,
            outputRequiredPathText: null,
            IndecesPostfix: "Indices",
            defaultIndicesFiles: ["archiveIndex.xml","contextDocumentationIndex.xml"],
            IndecesPath: null,
            outputOkSpn: null,
            outputOkText: null,
            selectDeliveryPackage: null,
            outputOkInformationTitle: null,
            outputOkInformationText: null,
            contextDocumentsTab: null,
            outputWrongFileNameTitle: null,
            outputWrongFileNameText: null
        }

        //reset status & input fields
        var Reset = function () {
            settings.outputErrorSpn.hidden = true;
            settings.outputOkSpn.hidden = true;
            settings.selectDeliveryPackage.hidden = true;            
        }

        //get selcted file name
        var GetFileName = function(selectedPath) {
            var filePath = selectedPath;
            var folders = filePath.getFolders();
            return folders[folders.length - 1];
        }

        //redirect
        var NextTab = function() {
            settings.outputOkSpn.hidden = false;
            var selectedArchiveIndexFileName = GetFileName(settings.selectedArchiveIndexFilePath);
            var selectedContextDocumentationIndexFileName = GetFileName(settings.selectedContextDocumentationIndexFilePath);
            settings.outputOkSpn.innerHTML =  settings.outputOkText.format(selectedArchiveIndexFileName,selectedContextDocumentationIndexFileName);
            settings.selectDeliveryPackage.innerHTML = "[{0}]".format(settings.IndecesPath);
            var filePath = (settings.IndecesPath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.IndecesPath,selectedContextDocumentationIndexFileName) : "{0}/{1}".format(settings.IndecesPath,selectedContextDocumentationIndexFileName);
            Rigsarkiv.Hybris.ContextDocuments.callback().load(fs.readFileSync(filePath));
            //ipcRenderer.send('open-information-dialog',settings.outputOkInformationTitle.innerHTML,settings.outputOkInformationText.innerHTML);
            settings.contextDocumentsTab.click();
        }

        //upload Indeces file 
        var EnsureFile = function (filePath) {
            var result = true;
            try 
            {
                var fileName = GetFileName(filePath);
                console.logInfo("copy file " + fileName + " to  " + settings.IndecesPath,"Rigsarkiv.Hybris.IndexFiles.EnsureFile");
                var destFilePath = settings.IndecesPath;
                destFilePath += (destFilePath.indexOf("\\") > -1) ? "\\{0}".format(fileName) : "/{0}".format(fileName);
                fs.copyFileSync(filePath,destFilePath);    
            }
            catch(err) {
                result = false;
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.IndexFiles.EnsureFile");
            }
            return result;
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.selectArchiveIndexFileBtn.addEventListener('click', (event) => {
                ipcRenderer.send('indexfiles-open-archiveindex-file-dialog');
            })
            ipcRenderer.on('indexfiles-selected-archiveindex-file', (event, path) => {
                settings.selectedArchiveIndexFilePath = path[0]; 
                console.logInfo(`selected ArchiveIndex path: ${path}`,"Rigsarkiv.Hybris.IndexFiles.AddEvents"); 
                settings.pathArchiveIndexFileTxt.value = settings.selectedArchiveIndexFilePath;            
            })
            settings.selectContextDocumentationIndexFileBtn.addEventListener('click', (event) => {
                ipcRenderer.send('indexfiles-open-contextdocumentationindex-file-dialog');
            })
            ipcRenderer.on('indexfiles-selected-contextdocumentationindex-file', (event, path) => {
                settings.selectedContextDocumentationIndexFilePath = path[0]; 
                console.logInfo(`selected ContextDocumentationIndex path: ${path}`,"Rigsarkiv.Hybris.IndexFiles.AddEvents"); 
                settings.pathContextDocumentationIndexFileTxt.value = settings.selectedContextDocumentationIndexFilePath;            
            })
            settings.okBtn.addEventListener('click', function (event) {
                Reset();
                if(settings.pathArchiveIndexFileTxt.value === "" || settings.pathContextDocumentationIndexFileTxt.value === "") {
                    ipcRenderer.send('open-error-dialog',settings.outputRequiredPathTitle.innerHTML,settings.outputRequiredPathText.innerHTML);
                }
                if(settings.selectedArchiveIndexFilePath != null && settings.selectedContextDocumentationIndexFilePath != null && settings.pathArchiveIndexFileTxt.value !== "" && settings.pathContextDocumentationIndexFileTxt.value !== "") { 
                    if(GetFileName(settings.selectedArchiveIndexFilePath) === settings.defaultIndicesFiles[0] && GetFileName(settings.selectedContextDocumentationIndexFilePath) === settings.defaultIndicesFiles[1]) {
                        var redirect = true;
                        settings.IndecesPath = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath;
                        settings.IndecesPath += (settings.IndecesPath.indexOf("\\") > -1) ? "\\{0}".format(settings.IndecesPostfix) : "/{0}".format(settings.IndecesPostfix);            
                        if(Rigsarkiv.Hybris.Base.callback().mode === "Edit") {
                            var archiveIndexPath = (settings.IndecesPath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.IndecesPath,settings.defaultIndicesFiles[0]) : "{0}/{1}".format(settings.IndecesPath,settings.defaultIndicesFiles[0]);
                            var contextDocumentationIndexPath = (settings.IndecesPath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.IndecesPath,settings.defaultIndicesFiles[1]) : "{0}/{1}".format(settings.IndecesPath,settings.defaultIndicesFiles[1]);
                            if(settings.selectedArchiveIndexFilePath !== archiveIndexPath) { redirect = EnsureFile(settings.selectedArchiveIndexFilePath); }
                            if(settings.selectedContextDocumentationIndexFilePath !== contextDocumentationIndexPath && redirect) { redirect = EnsureFile(settings.selectedContextDocumentationIndexFilePath); }
                        }
                        else {
                            redirect = (EnsureFile(settings.selectedArchiveIndexFilePath) && EnsureFile(settings.selectedContextDocumentationIndexFilePath));
                        }
                        if(redirect) { NextTab(); }
                    }
                    else {
                        ipcRenderer.send('open-error-dialog',settings.outputWrongFileNameTitle.innerHTML,settings.outputWrongFileNameText.innerHTML);    
                    }
                }
            })
            settings.selectDeliveryPackage.addEventListener('click', (event) => {
                ipcRenderer.send('open-item',settings.IndecesPath);
            }) 
        }

        //Model interfaces functions
        Rigsarkiv.Hybris.IndexFiles = {
            initialize: function (selectArchiveIndexFileId,pathArchiveIndexFileId,selectContextDocumentationIndexFileId,pathContextDocumentationIndexFileId,indexFilesOkBtn,outputErrorId,outputRequiredPathId,outputOkId,selectDeliveryPackageId,outputOkInformationPrefixId,contextDocumentsTabId,outputWrongFileNameId) {
                settings.selectArchiveIndexFileBtn = document.getElementById(selectArchiveIndexFileId);
                settings.pathArchiveIndexFileTxt = document.getElementById(pathArchiveIndexFileId);
                settings.selectContextDocumentationIndexFileBtn = document.getElementById(selectContextDocumentationIndexFileId);
                settings.pathContextDocumentationIndexFileTxt = document.getElementById(pathContextDocumentationIndexFileId);
                settings.okBtn = document.getElementById(indexFilesOkBtn);
                settings.outputErrorSpn =  document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.outputRequiredPathTitle =  document.getElementById(outputRequiredPathId + "-Title");
                settings.outputRequiredPathText =  document.getElementById(outputRequiredPathId + "-Text");
                settings.outputOkSpn =  document.getElementById(outputOkId);
                settings.outputOkText = settings.outputOkSpn.innerHTML;
                settings.selectDeliveryPackage = document.getElementById(selectDeliveryPackageId);
                settings.outputOkInformationTitle = document.getElementById(outputOkInformationPrefixId + "-Title");
                settings.outputOkInformationText = document.getElementById(outputOkInformationPrefixId + "-Text");
                settings.contextDocumentsTab = document.getElementById(contextDocumentsTabId);
                settings.outputWrongFileNameTitle = document.getElementById(outputWrongFileNameId + "-Title");
                settings.outputWrongFileNameText = document.getElementById(outputWrongFileNameId + "-Text");
                AddEvents();
            },
            callback: function () {
                return {
                    reset: function()  {
                        Reset();
                        settings.selectedArchiveIndexFilePath = null;
                        settings.selectedContextDocumentationIndexFilePath = null;
                        settings.pathArchiveIndexFileTxt.value = "";
                        settings.pathContextDocumentationIndexFileTxt.value = "";
                    },
                    load: function(pathArchiveIndexFile,pathContextDocumentationIndexFile) {
                        settings.selectedArchiveIndexFilePath = pathArchiveIndexFile;
                        settings.selectedContextDocumentationIndexFilePath = pathContextDocumentationIndexFile;
                        settings.pathArchiveIndexFileTxt.value = settings.selectedArchiveIndexFilePath;
                        settings.pathContextDocumentationIndexFileTxt.value = settings.selectedContextDocumentationIndexFilePath;
                    }
                }
            }
        }        
    }(jQuery);
}(jQuery);