window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron')
    const fs = require('fs');

    var settings = { 
        selectDirBtn: null,
        pathDirTxt: null
    }

    var AddEvents = function () {
        settings.selectDirBtn.addEventListener('click', (event) => {
            ipcRenderer.send('open-file-dialog');
        })
        ipcRenderer.on('selected-directory', (event, path) => {
            settings.selectedPath = path; 
            console.log(`selected path: ${path}`); 
            settings.pathDirTxt.value = settings.selectedPath;
         })
    }

    Rigsarkiv.Validation = {        
        initialize: function (selectDirectoryId,pathDirectoryId) {            
            settings.selectDirBtn =  document.getElementById(selectDirectoryId);
            settings.pathDirTxt =  document.getElementById(pathDirectoryId);
            AddEvents();
        }
    };

}(jQuery);