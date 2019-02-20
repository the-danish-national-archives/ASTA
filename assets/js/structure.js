'use strict';
var Rigsarkiv;
(function (Rigsarkiv) {
  (function (Structure) {
    
    const {ipcRenderer} = require('electron')
    const fs = require('fs');
    const pattern = /^[0-9]*$/;

    var settings = {
        selectDirBtn: null,
        pathDirTxt: null,
        selectedPath: null,
        deliveryPackageTxt: null,
        okBtn: null,
        outputSpn: null,
        folderPrefix: "FD.",
        subFolders: ["ContextDocumentation","Data","Indices"]
    }

    var Reset = function () {
        settings.outputSpn.innerHTML = "";
    }

    var EnsureStructure = function () {
        var folderPath = settings.selectedPath + "\\" + settings.folderPrefix + settings.deliveryPackageTxt.value        
        fs.exists(folderPath, (exists) => {
            if(!exists) {
                console.log("Create structure: " + folderPath);
                fs.mkdir(folderPath, { recursive: true }, (err) => {
                    if (err) {
                        settings.outputSpn.innerHTML = settings.outputSpn.innerHTML + "<br/>Error: " + err.message;
                    }
                });
                settings.subFolders.forEach(element => {
                    fs.mkdir(folderPath + "\\" + element, { recursive: true }, (err) => {
                        if (err) {
                            settings.outputSpn.innerHTML = settings.outputSpn.innerHTML + "<br/>Error: " + err.message;
                        }
                    });
                });
                settings.outputSpn.innerHTML = settings.outputSpn.innerHTML + "<br/>Afleveringspakken er oprettet" ;
            }
            else  {
                settings.outputSpn.innerHTML = settings.outputSpn.innerHTML + "<br/>Afleveringspakken eksisterer";
            }
        });
    }

    var AddEvents = function () {
        settings.okBtn.addEventListener('click', (event) => {
            Reset();
            if(settings.pathDirTxt.value === "") {
                settings.outputSpn.innerHTML = settings.outputSpn.innerHTML + "<br/>Destination for afleveringspakken mangler";
            }
            if(settings.deliveryPackageTxt.value === "") {
                settings.outputSpn.innerHTML = settings.outputSpn.innerHTML + "<br/>Løbenummer for afleveringspakken mangler";
            }
            if(settings.deliveryPackageTxt.value !== "" && !pattern.test(settings.deliveryPackageTxt.value)) {
                settings.outputSpn.innerHTML = settings.outputSpn.innerHTML + "<br/>Løbenummer for afleveringspakken ikke valid nummer";
            }
            if(settings.selectedPath != null && settings.pathDirTxt.value !== "" && settings.deliveryPackageTxt.value !== "" && pattern.test(settings.deliveryPackageTxt.value)) {
               EnsureStructure();
            }
        })
        settings.selectDirBtn.addEventListener('click', (event) => {
            Reset();
            ipcRenderer.send('open-file-dialog');
        })
         ipcRenderer.on('selected-directory', (event, path) => {
            settings.selectedPath = path; 
            console.log(`selected path: ${path}`); 
            settings.pathDirTxt.value = settings.selectedPath;
         })
    }

    Structure.initialize = function (selectDirectoryId,pathDirectoryId,deliveryPackageId,okId,outputId) {
        
        settings.selectDirBtn =  document.getElementById(selectDirectoryId);
        settings.pathDirTxt =  document.getElementById(pathDirectoryId);
        settings.deliveryPackageTxt =  document.getElementById(deliveryPackageId);
        settings.okBtn =  document.getElementById(okId);
        settings.outputSpn =  document.getElementById(outputId);
        AddEvents();       
    };
  })(Rigsarkiv.Structure || (Rigsarkiv.Structure = {}));  
})(Rigsarkiv || (Rigsarkiv = {}));

Rigsarkiv.Structure.initialize("hybris-select-directory","hybris-path","hybris-delivery-package","hybris-okBtn","hybris-output");
