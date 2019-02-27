window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron');

    var settings = {
        fileName: null,
        fileDescr: null,
        keyVar: null,
        foreignFileName: null,
        foreignKeyVarName: null,
        foreignFileRefVar: null,
        okBtn: null
    }

    var AddEvents = function () {
        settings.okBtn.addEventListener('click', function(event){
            console.log('file: ' + settings.fileName.value);
        })
    }

    Rigsarkiv.MetaData = {
        initialize: function (metadataFileName, metadataFileNameDescription, metadataKeyVariable, metadataForeignFileName, metadataForeignKeyVariableName, metadataReferenceVariable, metdataOkBtn){
            settings.fileName = document.getElementById(metadataFileName);
            settings.fileDescr = document.getElementById(metadataFileNameDescription);
            settings.keyVar = document.getElementById(metadataKeyVariable);  
            settings.foreignFileName = document.getElementById(metadataForeignFileName);
            settings.foreignKeyVarName =  document.getElementById(metadataForeignKeyVariableName);
            settings.foreignFileRefVar = document.getElementById(metadataReferenceVariable);
            settings.okBtn = document.getElementById(metdataOkBtn);
            AddEvents();
        }
    }
}(jQuery);