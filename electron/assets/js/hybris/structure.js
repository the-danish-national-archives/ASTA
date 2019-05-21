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
            selectDirBtn: null,
            pathDirTxt: null,
            selectedPath: null,
            deliveryPackageTxt: null,
            okBtn: null,
            outputErrorSpn: null,
            outputErrorText: null,
            outputExistsSpn: null,
            outputExistsTitle:null,
            outputExistsText: null,
            outputRequiredPathTitle: null,
            outputRequiredPathText: null,
            outputUnvalidDeliveryPackageTitle: null,
            outputUnvalidDeliveryPackageText: null,
            outputOkSpn: null,
            outputOkText: null,
            outputStatisticsHeaderTrin1Spn: null,
            outputStatisticsHeaderTrin1Text: null,
            outputStatisticsHeaderTrin2Spn: null,
            outputStatisticsHeaderTrin2Text: null,
            outputStatisticsHeaderTrin3Spn: null,
            outputStatisticsHeaderTrin3Text: null,
            outputStatisticsHeaderInformation2Spn: null,
            outputStatisticsHeaderInformation2Text: null,
            outputStatisticsHeaderindexfilesSpn: null,
            outputStatisticsHeaderindexfilesText: null,
            outputStatisticsHeadercontextdocumentsSpn: null,
            outputStatisticsHeadercontextdocumentsText: null,
            outputStatisticsHeaderOverviewSpn: null,
            outputStatisticsHeaderOverviewText: null,
            statisticsTab: null,
            selectDeliveryPackage: null,
            folderPrefix: "FD.",
            defaultFolderPostfix: "99999",
            subFolders: ["ContextDocumentation","Data","Indices"],
            deliveryPackagePath: null
        }

        //reset status & input fields
        var Reset = function () {
            settings.outputErrorSpn.hidden = true;
            settings.outputOkSpn.hidden = true;
            settings.selectDeliveryPackage.hidden = true;
        }

        //Output structure creation status
        var ShowOutput = function() {
            var folders = settings.deliveryPackagePath.getFolders();
            var folderName = folders[folders.length - 1];
            settings.selectDeliveryPackage.innerHTML = "[{0}]".format(settings.selectedPath);
            settings.selectDeliveryPackage.hidden = false;
            settings.outputOkSpn.hidden = false;
            settings.outputOkSpn.innerHTML = settings.outputOkText.format(folderName);
        }

        //create delivery Package folder Structure
        var EnsureStructure = function () {
            var folderName = settings.folderPrefix;
            folderName += (settings.deliveryPackageTxt.value === "") ? settings.defaultFolderPostfix: settings.deliveryPackageTxt.value; 
            settings.deliveryPackagePath = settings.selectedPath[0];      
            settings.deliveryPackagePath += (settings.deliveryPackagePath.indexOf("\\") > -1) ? "\\{0}".format(folderName) : "/{0}".format(folderName);
            fs.exists(settings.deliveryPackagePath, (exists) => {
                if(!exists) {
                    console.log("Create structure: " + settings.deliveryPackagePath);
                    fs.mkdir(settings.deliveryPackagePath, { recursive: true }, (err) => {
                        if (err) {
                            settings.outputErrorSpn.hidden = false;
                            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);
                        }
                        else {
                            ShowOutput();
                            settings.subFolders.forEach(element => {
                                var subFolderName = (settings.deliveryPackagePath.indexOf("\\") > -1) ? "\\{0}".format(element) : "/{0}".format(element);
                                fs.mkdir(settings.deliveryPackagePath + subFolderName, { recursive: true }, (err) => {
                                    if (err) {
                                        settings.outputErrorSpn.hidden = false;
                                        settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);   
                                        settings.deliveryPackagePath = null;                         
                                    }
                                });
                            });                        
                        }
                    });
                }
                else  {
                    ipcRenderer.send('open-warning-dialog',settings.outputExistsTitle.innerHTML,settings.outputExistsText.format(folderName));
                }
            });
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.okBtn.addEventListener('click', (event) => {
                Reset();
                if(settings.pathDirTxt.value === "") {
                    ipcRenderer.send('open-error-dialog',settings.outputRequiredPathTitle.innerHTML,settings.outputRequiredPathText.innerHTML);
                }
                if(settings.deliveryPackageTxt.value !== "" && !pattern.test(settings.deliveryPackageTxt.value)) {
                    ipcRenderer.send('open-error-dialog',settings.outputUnvalidDeliveryPackageTitle.innerHTML,settings.outputUnvalidDeliveryPackageText.innerHTML);
                }
                if(settings.selectedPath != null && settings.pathDirTxt.value !== "" && (settings.deliveryPackageTxt.value === "" || (settings.deliveryPackageTxt.value !== "" && pattern.test(settings.deliveryPackageTxt.value)))) {
                    EnsureStructure();
                    var folders = settings.deliveryPackagePath.getFolders(); 
                    var folder = folders[folders.length - 1];           
                    settings.outputStatisticsHeaderTrin1Spn.innerHTML = settings.outputStatisticsHeaderTrin1Text.format(folder);
                    settings.outputStatisticsHeaderTrin2Spn.innerHTML = settings.outputStatisticsHeaderTrin2Text.format(folder);
                    settings.outputStatisticsHeaderTrin3Spn.innerHTML = settings.outputStatisticsHeaderTrin3Text.format(folder);
                    settings.outputStatisticsHeaderInformation2Spn.innerHTML = settings.outputStatisticsHeaderInformation2Text.format(folder);
                    settings.outputStatisticsHeaderindexfilesSpn.innerHTML = settings.outputStatisticsHeaderindexfilesText.format(folder);
                    settings.outputStatisticsHeadercontextdocumentsSpn.innerHTML = settings.outputStatisticsHeadercontextdocumentsText.format(folder);
                    settings.outputStatisticsHeaderOverviewSpn.innerHTML = settings.outputStatisticsHeaderOverviewText.format(folder);
                    settings.statisticsTab.click();
                }
            });
            settings.selectDirBtn.addEventListener('click', (event) => {
                Reset();
                ipcRenderer.send('structure-open-file-dialog');
            });
            ipcRenderer.on('structure-selected-directory', (event, path) => {
                settings.selectedPath = path; 
                console.log(`selected path: ${path}`); 
                settings.pathDirTxt.value = settings.selectedPath;
            });
            settings.selectDeliveryPackage.addEventListener('click', (event) => {
                var folderPath = settings.selectedPath[0];
                ipcRenderer.send('open-item',folderPath);
            }); 
        }

        //Model interfaces functions
        Rigsarkiv.Hybris.Structure = {        
            initialize: function (selectDirectoryId,pathDirectoryId,deliveryPackageId,okId,outputErrorId,outputExistsId,outputRequiredPathId,outputUnvalidDeliveryPackageId,outputOkId,selectDeliveryPackageId,statisticsTabId,outputStatisticsHeaderTrin1,outputStatisticsHeaderTrin2,outputStatisticsHeaderTrin3,outputStatisticsHeaderInformation2,outputStatisticsHeaderindexfiles,outputStatisticsHeadercontextdocuments,outputStatisticsHeaderOverview) {            
                settings.selectDirBtn =  document.getElementById(selectDirectoryId);
                settings.pathDirTxt =  document.getElementById(pathDirectoryId);
                settings.deliveryPackageTxt =  document.getElementById(deliveryPackageId);
                settings.okBtn =  document.getElementById(okId);
                settings.outputErrorSpn =  document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.outputExistsTitle =  document.getElementById(outputExistsId + "-Title");
                settings.outputExistsText = document.getElementById(outputExistsId + "-Text").innerHTML;
                settings.outputRequiredPathTitle =  document.getElementById(outputRequiredPathId + "-Title");
                settings.outputRequiredPathText =  document.getElementById(outputRequiredPathId + "-Text");
                settings.outputUnvalidDeliveryPackageTitle =  document.getElementById(outputUnvalidDeliveryPackageId + "-Title");
                settings.outputUnvalidDeliveryPackageText =  document.getElementById(outputUnvalidDeliveryPackageId + "-Text");
                settings.outputOkSpn =  document.getElementById(outputOkId);
                settings.outputOkText = settings.outputOkSpn.innerHTML;
                settings.selectDeliveryPackage = document.getElementById(selectDeliveryPackageId);
                settings.statisticsTab = document.getElementById(statisticsTabId);
                settings.outputStatisticsHeaderTrin1Spn = document.getElementById(outputStatisticsHeaderTrin1);
                settings.outputStatisticsHeaderTrin1Text = settings.outputStatisticsHeaderTrin1Spn.innerHTML;
                settings.outputStatisticsHeaderTrin2Spn = document.getElementById(outputStatisticsHeaderTrin2);
                settings.outputStatisticsHeaderTrin2Text = settings.outputStatisticsHeaderTrin1Spn.innerHTML;
                settings.outputStatisticsHeaderTrin3Spn = document.getElementById(outputStatisticsHeaderTrin3);
                settings.outputStatisticsHeaderTrin3Text = settings.outputStatisticsHeaderTrin1Spn.innerHTML;
                settings.outputStatisticsHeaderInformation2Spn = document.getElementById(outputStatisticsHeaderInformation2);
                settings.outputStatisticsHeaderInformation2Text = settings.outputStatisticsHeaderInformation2Spn.innerHTML;
                settings.outputStatisticsHeaderindexfilesSpn = document.getElementById(outputStatisticsHeaderindexfiles);
                settings.outputStatisticsHeaderindexfilesText = settings.outputStatisticsHeaderindexfilesSpn.innerHTML;
                settings.outputStatisticsHeadercontextdocumentsSpn = document.getElementById(outputStatisticsHeadercontextdocuments);
                settings.outputStatisticsHeadercontextdocumentsText = settings.outputStatisticsHeadercontextdocumentsSpn.innerHTML;
                settings.outputStatisticsHeaderOverviewSpn = document.getElementById(outputStatisticsHeaderOverview);
                settings.outputStatisticsHeaderOverviewText = settings.outputStatisticsHeaderOverviewSpn.innerHTML;
                AddEvents();
            },
            callback: function () {
                var folderPath = (settings.selectedPath != null ? settings.selectedPath[0] : null);
                return { 
                    deliveryPackagePath: settings.deliveryPackagePath, 
                    selectedPath: folderPath,
                    reset: function() 
                    { 
                        settings.pathDirTxt.value = "";
                        settings.deliveryPackageTxt.value = "";
                        Reset();
                    }  
                };
            }
        };
    }(jQuery);    
}(jQuery);