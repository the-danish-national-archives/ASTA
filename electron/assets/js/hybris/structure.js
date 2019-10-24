/*
    Model is responsible for Ensure Delivery Package folder Structure
    initialize interface inputs: elements from <div id="hybris-panel-structure">
    callback interface outputs:  delivery Package relative Path & selected absolut Path
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
    function (n){
        const {ipcRenderer} = require('electron')
        const fs = require('fs');
        const pattern = /^([1-9]{1}[0-9]{4,})$/;

        //private data memebers
        var settings = {            
            sectionTitleH1: null,
            titleNewSpn: null,
            titleEditSpn: null,
            selectNewDirBtn: null,
            selectEditDirBtn: null,
            newPathDirTxt: null,
            editPathDirTxt: null,
            selectedPath: null,
            deliveryPackageTxt: null,
            okNewBtn: null,
            okEditBtn: null,
            outputErrorSpn: null,
            outputErrorText: null,
            outputExistsSpn: null,
            outputOkSpn: null,
            outputStatisticsHeaderTrin1Spn: null,
            outputStatisticsHeaderTrin2Spn: null,
            outputStatisticsHeaderTrin3Spn: null,
            outputStatisticsHeaderInformation2Spn: null,
            outputStatisticsHeaderReferencesSpn: null,
            outputStatisticsHeaderindexfilesSpn: null,
            outputStatisticsHeadercontextdocumentsSpn: null,
            outputStatisticsHeaderOverviewSpn: null,
            structureTab: null,
            statisticsTab: null,
            indexfilesTab: null,
            newPanelDiv: null,
            editPanelDiv: null,
            indexFilesDescriptionSpn: null,
            selectDeliveryPackage: null,
            indecesFolder: "Indices",
            contextDocumentationFolder: "ContextDocumentation",
            defaultIndicesFiles: ["archiveIndex.xml","contextDocumentationIndex.xml"],
            folderPrefix: "FD.",
            defaultFolderPostfix: "99999",
            subFolders: ["ContextDocumentation","Data","Indices"],
            foldersCounter: 0,
            deliveryPackagePath: null
        }

        //reset status & input fields
        var Reset = function () {
            settings.foldersCounter = 0;
            settings.outputErrorSpn.hidden = true;
            settings.outputOkSpn.hidden = true;
            settings.selectDeliveryPackage.hidden = true;
            Rigsarkiv.Hybris.DataExtraction.callback().reset();
            Rigsarkiv.Hybris.MetaData.callback().reset();
            Rigsarkiv.Hybris.References.callback().reset();
            Rigsarkiv.Hybris.IndexFiles.callback().reset();
        }

        //Output structure creation status & redirect
        var NextTab = function() {
            var folders = settings.deliveryPackagePath.getFolders();
            var folderName = folders[folders.length - 1];
            settings.selectDeliveryPackage.innerHTML = "[{0}]".format(settings.selectedPath);
            settings.selectDeliveryPackage.hidden = false;
            settings.outputOkSpn.hidden = false;
            
            var folder = folders[folders.length - 1];           
            settings.outputStatisticsHeaderTrin1Spn.innerHTML = folder;
            settings.outputStatisticsHeaderTrin2Spn.innerHTML = folder;
            settings.outputStatisticsHeaderTrin3Spn.innerHTML = folder;
            settings.outputStatisticsHeaderInformation2Spn.innerHTML = folder;
            settings.outputStatisticsHeaderReferencesSpn.innerHTML = folder;
            settings.outputStatisticsHeaderindexfilesSpn.innerHTML = folder;
            settings.outputStatisticsHeadercontextdocumentsSpn.innerHTML = folder;
            settings.outputStatisticsHeaderOverviewSpn.innerHTML = folder;
            
            if(Rigsarkiv.Hybris.Base.callback().mode === "New") { settings.statisticsTab.click(); }
            if(Rigsarkiv.Hybris.Base.callback().mode === "Edit") { 
                var description = "{0} {1}".format(folder,Rigsarkiv.Language.callback().getValue("hybris-structure-indexfiles-Description"));
                settings.indexFilesDescriptionSpn.innerHTML = Rigsarkiv.Language.callback().getValue("hybris-output-indexfiles-Description").format(description);
                settings.indexfilesTab.click(); 
            }
        }

        //create delivery Package folder Structure
        var EnsureStructure = function () {
            var folderName = settings.folderPrefix;
            folderName += (settings.deliveryPackageTxt.value === "") ? settings.defaultFolderPostfix: settings.deliveryPackageTxt.value; 
            settings.deliveryPackagePath = settings.selectedPath[0];      
            settings.deliveryPackagePath += (settings.deliveryPackagePath.indexOf("\\") > -1) ? "\\{0}".format(folderName) : "/{0}".format(folderName);
            fs.exists(settings.deliveryPackagePath, (exists) => {
                if(!exists) {
                    console.logInfo("Create structure: " + settings.deliveryPackagePath, "Rigsarkiv.Hybris.Structure.EnsureStructure");
                    fs.mkdir(settings.deliveryPackagePath, { recursive: true }, (err) => {
                        if (err) {
                            err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.Structure.EnsureStructure");
                        }
                        else {
                            settings.foldersCounter += 1;
                            settings.subFolders.forEach(element => {
                                var subFolderName = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "\\{0}".format(element) : "/{0}".format(element);
                                fs.mkdir(settings.deliveryPackagePath + subFolderName, { recursive: true }, (err) => {
                                    if (err) {
                                        err.Handle(settings.outputErrorSpn,settings.outputErrorText, "Rigsarkiv.Hybris.Structure.EnsureStructure");   
                                        settings.deliveryPackagePath = null;                         
                                    }
                                    else {
                                        settings.foldersCounter += 1;
                                        if(settings.foldersCounter === 4) {
                                            NextTab();
                                        }
                                    }
                                });
                            });                        
                        }
                    });
                }
                else  {
                    ipcRenderer.send('open-warning-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-structure-Exists-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-structure-Exists-Text").format(folderName));
                }
            });
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.okNewBtn.addEventListener('click', (event) => {
                Reset();
                if(settings.newPathDirTxt.value === "") {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-structure-RequiredPath-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-structure-RequiredPath-Text"));
                }
                if(settings.deliveryPackageTxt.value !== "" && !pattern.test(settings.deliveryPackageTxt.value)) {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-structure-UnvalidDeliveryPackage-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-structure-UnvalidDeliveryPackage-Text"));
                }
                if(settings.selectedPath != null && settings.newPathDirTxt.value !== "" && (settings.deliveryPackageTxt.value === "" || (settings.deliveryPackageTxt.value !== "" && pattern.test(settings.deliveryPackageTxt.value)))) {
                    EnsureStructure();
                }
            });
            settings.okEditBtn.addEventListener('click', (event) => {
                Reset(); 
                if(settings.editPathDirTxt.value === "") {
                    ipcRenderer.send('open-error-dialog',Rigsarkiv.Language.callback().getValue("hybris-output-structure-RequiredPath-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-structure-RequiredPath-Text"));
                }
                else {
                    settings.deliveryPackagePath = settings.selectedPath[0];
                    var contextDocumentationPath = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.deliveryPackagePath,settings.contextDocumentationFolder) : "{0}/{1}".format(settings.deliveryPackagePath,settings.contextDocumentationFolder);
                    var indecesPath = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.deliveryPackagePath,settings.indecesFolder) : "{0}/{1}".format(settings.deliveryPackagePath,settings.indecesFolder);
                    if(fs.existsSync(indecesPath) && fs.existsSync(contextDocumentationPath))
                    {
                        var archiveIndexPath = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "{0}\\{1}".format(indecesPath,settings.defaultIndicesFiles[0]) : "{0}/{1}".format(indecesPath,settings.defaultIndicesFiles[0]);
                        var contextDocumentationIndexPath = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "{0}\\{1}".format(indecesPath,settings.defaultIndicesFiles[1]) : "{0}/{1}".format(indecesPath,settings.defaultIndicesFiles[1]);
                        if(fs.existsSync(archiveIndexPath) && fs.existsSync(contextDocumentationIndexPath)) { Rigsarkiv.Hybris.IndexFiles.callback().load(archiveIndexPath,contextDocumentationIndexPath); }
                        NextTab();
                    }
                    else {
                        ipcRenderer.send('open-error-dialog',settings.requiredIndexFilesTitle.innerHTML,settings.requiredIndexFilesText.innerHTML);
                    }
                } 
            });
            settings.selectNewDirBtn.addEventListener('click', (event) => {
                Reset();
                ipcRenderer.send('structure-open-file-dialog');
            });
            settings.selectEditDirBtn.addEventListener('click', (event) => {
                Reset();
                ipcRenderer.send('structure-open-file-dialog');
            });
            ipcRenderer.on('structure-selected-directory', (event, path) => {
                settings.selectedPath = path; 
                console.logInfo(`selected path: ${path}`,"Rigsarkiv.Hybris.Structure.AddEvents"); 
                if(Rigsarkiv.Hybris.Base.callback().mode === "New") { settings.newPathDirTxt.value = settings.selectedPath; }
                if(Rigsarkiv.Hybris.Base.callback().mode === "Edit") { settings.editPathDirTxt.value = settings.selectedPath; }
            });
            settings.selectDeliveryPackage.addEventListener('click', (event) => {
                var folderPath = settings.selectedPath[0];
                ipcRenderer.send('open-item',folderPath);
            }); 
        }

        //Model interfaces functions
        Rigsarkiv.Hybris.Structure = {        
            initialize: function (sectionTitleId,titleNewId,titleEditId,selectNewDirectoryId,selectEditDirectoryId,newPathDirectoryId,editPathDirectoryId,deliveryPackageId,okNewId,okEditId,outputErrorId,outputOkId,selectDeliveryPackageId,structureTabId,statisticsTabId,indexfilesTabId,outputStatisticsHeaderTrin1,outputStatisticsHeaderTrin2,outputStatisticsHeaderTrin3,outputStatisticsHeaderInformation2,outputStatisticsHeaderReferences,outputStatisticsHeaderindexfiles,outputStatisticsHeadercontextdocuments,outputStatisticsHeaderOverview,modePanelId,indexFilesDescriptionId) {            
                settings.sectionTitleH1 =  document.getElementById(sectionTitleId);
                settings.titleNewSpn =  document.getElementById(titleNewId);
                settings.titleEditSpn =  document.getElementById(titleEditId);
                settings.selectNewDirBtn =  document.getElementById(selectNewDirectoryId);
                settings.selectEditDirBtn =  document.getElementById(selectEditDirectoryId);
                settings.newPathDirTxt =  document.getElementById(newPathDirectoryId);
                settings.editPathDirTxt =  document.getElementById(editPathDirectoryId);
                settings.deliveryPackageTxt =  document.getElementById(deliveryPackageId);
                settings.okNewBtn =  document.getElementById(okNewId);
                settings.okEditBtn =  document.getElementById(okEditId);
                settings.outputErrorSpn =  document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.outputOkSpn =  document.getElementById(outputOkId);
                settings.selectDeliveryPackage = document.getElementById(selectDeliveryPackageId);
                settings.structureTab = document.getElementById(structureTabId);
                settings.statisticsTab = document.getElementById(statisticsTabId);
                settings.indexfilesTab = document.getElementById(indexfilesTabId);
                settings.outputStatisticsHeaderTrin1Spn = document.getElementById(outputStatisticsHeaderTrin1);
                settings.outputStatisticsHeaderTrin2Spn = document.getElementById(outputStatisticsHeaderTrin2);
                settings.outputStatisticsHeaderTrin3Spn = document.getElementById(outputStatisticsHeaderTrin3);
                settings.outputStatisticsHeaderInformation2Spn = document.getElementById(outputStatisticsHeaderInformation2);
                settings.outputStatisticsHeaderReferencesSpn = document.getElementById(outputStatisticsHeaderReferences);
                settings.outputStatisticsHeaderindexfilesSpn = document.getElementById(outputStatisticsHeaderindexfiles);
                settings.outputStatisticsHeadercontextdocumentsSpn = document.getElementById(outputStatisticsHeadercontextdocuments);
                settings.outputStatisticsHeaderOverviewSpn = document.getElementById(outputStatisticsHeaderOverview);
                settings.newPanelDiv = document.getElementById(modePanelId + "-New");
                settings.editPanelDiv = document.getElementById(modePanelId + "-Edit");
                settings.indexFilesDescriptionSpn = document.getElementById(indexFilesDescriptionId);
                AddEvents();
            },
            callback: function () {
                var folderPath = (settings.selectedPath != null ? settings.selectedPath[0] : null);
                return { 
                    deliveryPackagePath: settings.deliveryPackagePath, 
                    selectedPath: folderPath,
                    reset: function() 
                    { 
                        settings.newPathDirTxt.value = "";
                        settings.deliveryPackageTxt.value = "";
                        settings.editPathDirTxt.value = "";
                        Reset();
                        if(Rigsarkiv.Hybris.Base.callback().mode === "New") {
                            settings.sectionTitleH1.innerText = settings.titleNewSpn.innerHTML;
                            $(settings.newPanelDiv).show();
                            $(settings.editPanelDiv).hide();
                        }
                        if(Rigsarkiv.Hybris.Base.callback().mode === "Edit") {
                            settings.sectionTitleH1.innerText = settings.titleEditSpn.innerHTML;
                            $(settings.newPanelDiv).hide();
                            $(settings.editPanelDiv).show();                            
                        }
                        settings.structureTab.click();
                    }
                };
            }
        };
    }(jQuery);    
}(jQuery);