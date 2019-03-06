window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron')
    const {shell} = require('electron')
    const fs = require('fs');
    const pattern = /^([1-9]{1}[0-9]{4,})$/;

    var settings = {
        selectDirBtn: null,
        pathDirTxt: null,
        selectedPath: null,
        deliveryPackageTxt: null,
        okBtn: null,
        nextBtn: null,
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
        statisticsTab: null,
        selectDeliveryPackage: null,
        folderPrefix: "FD.",
        defaultFolderPostfix: "99999",
        subFolders: ["ContextDocumentation","Data","Indices"],
        deliveryPackagePath: null
    }

    var Reset = function () {
        settings.outputErrorSpn.hidden = true;
        settings.outputOkSpn.hidden = true;
        settings.selectDeliveryPackage.hidden = true;
        settings.nextBtn.hidden = true;
    }

    var ShowOutput = function() {
        var folders = settings.deliveryPackagePath.split("/");
        var folderName = folders[folders.length - 1];
        settings.selectDeliveryPackage.innerHTML = settings.selectedPath;
        settings.selectDeliveryPackage.hidden = false;
        settings.outputOkSpn.hidden = false;
        settings.outputOkSpn.innerHTML = settings.outputOkText.format(folderName);
        settings.nextBtn.hidden = false;
    }

    var EnsureStructure = function () {
        var folderName = settings.folderPrefix;
        folderName += (settings.deliveryPackageTxt.value === "") ? settings.defaultFolderPostfix: settings.deliveryPackageTxt.value;        
        settings.deliveryPackagePath = settings.selectedPath[0].normlizePath() + "/" + folderName;
        fs.exists(settings.deliveryPackagePath, (exists) => {
            if(!exists) {
                console.log("Create structure: " + settings.deliveryPackagePath);
                fs.mkdir(settings.deliveryPackagePath, { recursive: true }, (err) => {
                    if (err) {
                        settings.outputErrorSpn.hidden = false;
                        settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);
                    }
                    else {
                        settings.subFolders.forEach(element => {
                            fs.mkdir(settings.deliveryPackagePath + "/" + element, { recursive: true }, (err) => {
                                if (err) {
                                    settings.outputErrorSpn.hidden = false;
                                    settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);   
                                    settings.deliveryPackagePath = null;                         
                                }
                                else {
                                    ShowOutput();
                                }
                            });
                        });                        
                    }
                });
            }
            else  {
                ipcRenderer.send('open-warning-dialog',settings.outputExistsTitle.innerHTML,settings.outputExistsText.format(folderName));
                ShowOutput();
            }
        });
    }

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
            }
        })
        settings.nextBtn.addEventListener('click', (event) => {
            settings.statisticsTab.click();
        });
        settings.selectDirBtn.addEventListener('click', (event) => {
            Reset();
            ipcRenderer.send('structure-open-file-dialog');
        })
        ipcRenderer.on('structure-selected-directory', (event, path) => {
            settings.selectedPath = path; 
            console.log(`selected path: ${path}`); 
            settings.pathDirTxt.value = settings.selectedPath;
         })
        settings.selectDeliveryPackage.addEventListener('click', (event) => {
           var folderPath = settings.selectedPath[0];
            shell.openItem(folderPath);
        }) 
    }

    Rigsarkiv.Structure = {        
        initialize: function (selectDirectoryId,pathDirectoryId,deliveryPackageId,okId,outputErrorId,outputExistsId,outputRequiredPathId,outputUnvalidDeliveryPackageId,outputOkId,selectDeliveryPackageId,nextId,statisticsTabId) {            
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
            settings.nextBtn =  document.getElementById(nextId);
            settings.statisticsTab = document.getElementById(statisticsTabId);
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