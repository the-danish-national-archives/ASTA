window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron');

    var settings = {
        fileName: null,
        fileDescr: null,
        keyVar: null,
        foreignFileName: null,
        foreignKeyVarName: null,
        foreignFileRefVar: null
    }

    Rigsarkiv.MetaData = {
        initialize: function (metadataFileName, metadataFileNameDescription, metadataKeyVariable, metadataForeignFileName, metadataForeignKeyVariableName, metadataReferenceVariable){
            settings.fileName = document.getElementById(metadataFileName);
            settings.fileDescr = document.getElementById(metadataFileNameDescription);
            settings.keyVar = document.getElementById(metadataKeyVariable);  
            settings.foreignFileName = document.getElementById(metadataForeignFileName);
            settings.foreignKeyVarName =  document.getElementById(metadataForeignKeyVariableName);
            settings.foreignFileRefVar = document.getElementById(metadataReferenceVariable);
        }
    }
}(jQuery);