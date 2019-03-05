window.Rigsarkiv = window.Rigsarkiv || {},
    function (n) {

        const { ipcRenderer } = require('electron');
        const {shell} = require('electron');
        const fs = require('fs');

        var settings = { 
            metadataCallback: null,
            selectArchiveIndexFileBtn: null,
            pathArchiveIndexFileTxt: null,
            selectedArchiveIndexFilePath: null,
            selectContextDocumentationIndexFileBtn: null,
            pathContextDocumentationIndexFileTxt: null,
            selectedContextDocumentationIndexFilePath: null,
            okBtn: null,
            outputErrorSpn: null,
            outputErrorText: null,
            outputRequiredPathSpn: null,
            IndecesPostfixPath: "{0}/Indices",
            IndecesPath: null,
            outputOkSpn: null,
            outputOkText: null,
            selectDeliveryPackage: null
         }

        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputErrorSpn.hidden = false;
            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);            
        }

        var Reset = function () {
            settings.outputErrorSpn.hidden = true;
            settings.outputRequiredPathSpn.hidden = true;
            settings.outputOkSpn.hidden = true;
            settings.selectDeliveryPackage.hidden = true;
        }

        var GetFileName = function(selectedPath) {
            var filePath = selectedPath[0].normlizePath();
            var folders = filePath.split("/");
            return folders[folders.length - 1];
        }

        var GetLocalFolderPath = function() {
            var folderPath = settings.structureCallback().selectedPath;
            if(folderPath.indexOf("\\") > -1) {
                var folders = settings.IndecesPath.split("/");
                return "{0}\\{1}\\{2}".format(folderPath,folders[folders.length - 2],folders[folders.length - 1]);
            }
            else {
                return settings.IndecesPath;
            }
        }

        var EnsureFiles= function () {
            var structureFolderPath = settings.structureCallback().deliveryPackagePath;
            settings.IndecesPath = settings.IndecesPostfixPath.format(structureFolderPath);
            var fileName = GetFileName(settings.selectedArchiveIndexFilePath);
            console.log("copy file " + fileName + " to  " + settings.IndecesPath);
            fs.copyFile(settings.selectedArchiveIndexFilePath[0], "{0}/{1}".format(settings.IndecesPath,fileName), (err) => {
                if (err) {
                    HandleError(err);
                }
                else {                   
                    var fileName = GetFileName(settings.selectedContextDocumentationIndexFilePath);
                    console.log("copy file " + fileName + " to  " + settings.IndecesPath);
                    fs.copyFile(settings.selectedContextDocumentationIndexFilePath[0], "{0}/{1}".format(settings.IndecesPath,fileName), (err) => {
                        if (err) {
                            HandleError(err);
                        }
                        else {
                            settings.outputOkSpn.hidden = false;
                            settings.selectDeliveryPackage.hidden = false;
                            var selectedArchiveIndexFileName = GetFileName(settings.selectedArchiveIndexFilePath);
                            var selectedContextDocumentationIndexFileName = GetFileName(settings.selectedContextDocumentationIndexFilePath);
                            settings.outputOkSpn.innerHTML =  settings.outputOkText.format(selectedArchiveIndexFileName,selectedContextDocumentationIndexFileName);
                            settings.selectDeliveryPackage.innerHTML = GetLocalFolderPath();
                        }
                    });
                }
            });
        }

        var AddEvents = function () {
            settings.selectArchiveIndexFileBtn.addEventListener('click', (event) => {
                ipcRenderer.send('indexfiles-open-archiveindex-file-dialog');
             })
             ipcRenderer.on('indexfiles-selected-archiveindex-file', (event, path) => {
                 settings.selectedArchiveIndexFilePath = path; 
                 console.log(`selected ArchiveIndex path: ${path}`); 
                 settings.pathArchiveIndexFileTxt.value = settings.selectedArchiveIndexFilePath;            
              })
              settings.selectContextDocumentationIndexFileBtn.addEventListener('click', (event) => {
                ipcRenderer.send('indexfiles-open-contextdocumentationindex-file-dialog');
             })
             ipcRenderer.on('indexfiles-selected-contextdocumentationindex-file', (event, path) => {
                settings.selectedContextDocumentationIndexFilePath = path; 
                console.log(`selected ContextDocumentationIndex path: ${path}`); 
                settings.pathContextDocumentationIndexFileTxt.value = settings.selectedContextDocumentationIndexFilePath;            
             })
             settings.okBtn.addEventListener('click', function (event) {
                Reset();
                if(settings.pathArchiveIndexFileTxt.value === "" || settings.pathContextDocumentationIndexFileTxt.value === "") {
                    settings.outputRequiredPathSpn.hidden = false;
                }
               if(settings.selectedArchiveIndexFilePath != null && settings.selectedContextDocumentationIndexFilePath != null && settings.pathArchiveIndexFileTxt.value !== "" && settings.pathContextDocumentationIndexFileTxt.value !== "") { 
                    EnsureFiles(); 
                }
            })
            settings.selectDeliveryPackage.addEventListener('click', (event) => {
                shell.openItem(GetLocalFolderPath());
             }) 
        }

        Rigsarkiv.IndexFiles = {
            initialize: function (structureCallback,selectArchiveIndexFileId,pathSArchiveIndexFileId,selectContextDocumentationIndexFileId,pathContextDocumentationIndexFileId,indexFilesOkBtn,outputErrorId,outputRequiredPathId,outputOkId,selectDeliveryPackageId) {
                settings.structureCallback = structureCallback;
                settings.selectArchiveIndexFileBtn = document.getElementById(selectArchiveIndexFileId);
                settings.pathArchiveIndexFileTxt = document.getElementById(pathSArchiveIndexFileId);
                settings.selectContextDocumentationIndexFileBtn = document.getElementById(selectContextDocumentationIndexFileId);
                settings.pathContextDocumentationIndexFileTxt = document.getElementById(pathContextDocumentationIndexFileId);
                settings.okBtn = document.getElementById(indexFilesOkBtn);
                settings.outputErrorSpn =  document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.outputRequiredPathSpn =  document.getElementById(outputRequiredPathId);
                settings.outputOkSpn =  document.getElementById(outputOkId);
                settings.outputOkText = settings.outputOkSpn.innerHTML;
                settings.selectDeliveryPackage = document.getElementById(selectDeliveryPackageId);
                AddEvents();
            }
        }
}(jQuery);