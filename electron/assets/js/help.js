/*
    Model is responsible for help (admin, none admin)
    initialize interface inputs: 
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {shell} = require('electron');
    const os = require('os');
    const log = require('electron-log');

    //private data members
    var settings = {
        outputErrorSpn: null,
        outputErrorText: null,
        rightsCallback: null,
        selectLogFolder: null,
        logPath: null
    }

    // Ensure help Data
    var EnsureData = function() {
        settings.logPath = log.transports.file.file;
        if(os.platform() == "win32") {
            settings.logPath = settings.logPath.substring(0,settings.logPath.lastIndexOf("\\"));
        }
        if(os.platform() == "darwin") {
            settings.logPath = settings.logPath.substring(0,settings.logPath.lastIndexOf("/"));
        }
        settings.selectLogFolder.innerHTML = settings.logPath;
    }

    //add Event Listener to HTML elmenets
    var AddEvents = function () {
        settings.selectLogFolder.addEventListener('click', (event) => {
            shell.openItem(settings.logPath);
        })
    }

    //Model interfaces functions
    Rigsarkiv.Help = {
        initialize: function (rightsCallback,outputErrorId,selectLogFolderId) {
            settings.rightsCallback = rightsCallback;
            settings.outputErrorSpn = document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML; 
            settings.selectLogFolder = document.getElementById(selectLogFolderId);
            try
            {
                EnsureData();
            }
            catch(err) 
            {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Help.initialize"); 
            }            
            AddEvents();            
        }
    }
}(jQuery);