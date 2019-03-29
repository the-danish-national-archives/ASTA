/*
    Model is responsible for Batch validation
    initialize interface inputs: elements from <div id="nemesis-batch-test-panel">
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Nemesis = Rigsarkiv.Nemesis || {},
    function (n) {
        const {ipcRenderer} = require('electron');
        const fs = require('fs');        

        //private data memebers
        var settings = { 
            outputErrorSpn: null,
            outputErrorText: null,
            selectDirBtn: null,
            pathDirTxt: null,
            selectedPath: null,
            validateBtn: null,
            structureCallback: null,
            deliveryPackages: [],
            runIndex: 0
        }          
          
        var Validate = function() {
            var deliveryPackagePath = settings.deliveryPackages[settings.runIndex];
            console.log(`validate path: ${deliveryPackagePath}`);                    
            settings.structureCallback().validate(deliveryPackagePath);
            settings.runIndex = settings.runIndex + 1;
            if(settings.runIndex < settings.deliveryPackages.length) {                
                setTimeout(Validate, 1000);
            }            
        }

        var Run = function () {
            var destPath = settings.selectedPath[0];
            fs.readdirSync(destPath).forEach(folder => {
                var deliveryPackagePath = (destPath.indexOf("\\") > -1) ? "{0}\\{1}".format(destPath,folder) : "{0}/{1}".format(destPath,folder);
                if(fs.statSync(deliveryPackagePath).isDirectory()) {
                    settings.deliveryPackages.push(deliveryPackagePath);
                }
            });
            if(settings.deliveryPackages.length > 0) {
                setTimeout(Validate, 1000);
            }            
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.validateBtn.addEventListener('click', (event) => {
                if(settings.selectedPath == null || settings.pathDirTxt.value === "") { return; }                
                Run();
            })
            settings.selectDirBtn.addEventListener('click', (event) => {
                ipcRenderer.send('batch-open-file-dialog');
            })
            ipcRenderer.on('batch-selected-directory', (event, path) => {
                settings.selectedPath = path; 
                console.log(`selected path: ${path}`); 
                settings.pathDirTxt.value = settings.selectedPath;
            })            
        }

        //Model interfaces functions
        Rigsarkiv.Nemesis.Batch = { 
            initialize: function (structureCallback,outputErrorId,selectDirectoryId,pathDirectoryId,validateId) {            
                settings.structureCallback = structureCallback;
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.selectDirBtn = document.getElementById(selectDirectoryId);
                settings.pathDirTxt = document.getElementById(pathDirectoryId);
                settings.validateBtn = document.getElementById(validateId);                
                AddEvents();
            }
        };

    }(jQuery);
}(jQuery);