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
        outputErrorSpn: null,
        outputErrorText: null,
        outputExistsSpn: null,
        outputExistsText: null,
        outputRequiredPathSpn: null,
        outputUnvalidDeliveryPackageSpn: null,
        outputOkSpn: null,
        outputOkText: null,
        outputSupplementSpn: null,
        selectDeliveryPackage: null,
        folderPrefix: "FD.",
        defaultFolderPostfix: "99999",
        subFolders: ["ContextDocumentation","Data","Indices"],
        deliveryPackagePath: null
    }

    var Reset = function () {
        settings.outputErrorSpn.hidden = true;
        settings.outputExistsSpn.hidden = true;
        settings.outputRequiredPathSpn.hidden = true;
        settings.outputUnvalidDeliveryPackageSpn.hidden = true;
        settings.outputOkSpn.hidden = true;
        settings.selectDeliveryPackage.hidden = true;
        settings.outputSupplementSpn.hidden = true;
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
                            });
                        });
                        settings.selectDeliveryPackage.innerHTML = settings.selectedPath;
                        settings.selectDeliveryPackage.hidden = false;
                        settings.outputOkSpn.hidden = false;
                        settings.outputSupplementSpn.hidden = false;
                        settings.outputOkSpn.innerHTML = settings.outputOkText.format(folderName);
                    }
                });
            }
            else  {
                settings.outputExistsSpn.hidden = false;
                settings.outputExistsSpn.innerHTML = settings.outputExistsText.format(folderName);
            }
        });
    }

    var AddEvents = function () {
        settings.okBtn.addEventListener('click', (event) => {
            Reset();
            if(settings.pathDirTxt.value === "") {
                settings.outputRequiredPathSpn.hidden = false;
            }
            if(settings.deliveryPackageTxt.value !== "" && !pattern.test(settings.deliveryPackageTxt.value)) {
                settings.outputUnvalidDeliveryPackageSpn.hidden = false;
            }
            if(settings.selectedPath != null && settings.pathDirTxt.value !== "" && (settings.deliveryPackageTxt.value === "" || (settings.deliveryPackageTxt.value !== "" && pattern.test(settings.deliveryPackageTxt.value)))) {
               EnsureStructure();
            }
        })
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
        initialize: function (selectDirectoryId,pathDirectoryId,deliveryPackageId,okId,outputErrorId,outputExistsId,outputRequiredPathId,outputUnvalidDeliveryPackageId,outputOkId,outputSupplementId,selectDeliveryPackageId) {            
            settings.selectDirBtn =  document.getElementById(selectDirectoryId);
            settings.pathDirTxt =  document.getElementById(pathDirectoryId);
            settings.deliveryPackageTxt =  document.getElementById(deliveryPackageId);
            settings.okBtn =  document.getElementById(okId);
            settings.outputErrorSpn =  document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML;
            settings.outputExistsSpn =  document.getElementById(outputExistsId);
            settings.outputExistsText = settings.outputExistsSpn.innerHTML;
            settings.outputRequiredPathSpn =  document.getElementById(outputRequiredPathId);
            settings.outputUnvalidDeliveryPackageSpn =  document.getElementById(outputUnvalidDeliveryPackageId);
            settings.outputOkSpn =  document.getElementById(outputOkId);
            settings.outputOkText = settings.outputOkSpn.innerHTML;
            settings.outputSupplementSpn = document.getElementById(outputSupplementId);
            settings.selectDeliveryPackage = document.getElementById(selectDeliveryPackageId);
            AddEvents();
        },
        callback: function () {
            return { deliveryPackagePath: settings.deliveryPackagePath, selectedPath: settings.selectedPath[0] };
        }
    };
}(jQuery);