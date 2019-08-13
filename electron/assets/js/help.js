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
        logPath: null,
        adminPanel: null,
        selectAthenaLogFolder: null,
        logAthenaPath: null,
        packagePath: "\\app-1.0.0\\resources"
    }

    // Ensure help Data
    var EnsureData = function() {
        var folders = log.transports.file.file.getFolders();
        settings.adminPanel.hidden = !settings.rightsCallback().isAdmin;
        settings.logPath = log.transports.file.file;
        if(os.platform() == "win32") {
            settings.logPath = settings.logPath.substring(0,settings.logPath.lastIndexOf("\\"));
        }
        if(os.platform() == "darwin") {
            settings.logPath = settings.logPath.substring(0,settings.logPath.lastIndexOf("/"));
        }
        settings.selectLogFolder.innerHTML = settings.logPath;
        for(var i = 0; i < folders.length; i++) {
            if(folders[i] === "Roaming") { folders[i] = "Local"; }
        }
        settings.logAthenaPath = folders.join("\\");
        settings.logAthenaPath = settings.logAthenaPath.substring(0,settings.logAthenaPath.lastIndexOf("\\"));
        settings.logAthenaPath += settings.packagePath;
        settings.selectAthenaLogFolder.innerHTML = settings.logAthenaPath;
    }

    //add Event Listener to HTML elmenets
    var AddEvents = function () {
        settings.selectLogFolder.addEventListener('click', (event) => {
            shell.openItem(settings.logPath);
        })
        settings.selectAthenaLogFolder.addEventListener('click', (event) => {
            shell.openItem(settings.logAthenaPath);
        })
    }

    //Model interfaces functions
    Rigsarkiv.Help = {
        initialize: function (rightsCallback,outputErrorId,selectLogFolderId,adminPanelId,selectAthenaLogFolderId) {
            settings.rightsCallback = rightsCallback;
            settings.outputErrorSpn = document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML; 
            settings.selectLogFolder = document.getElementById(selectLogFolderId);
            settings.adminPanel =  document.getElementById(adminPanelId);
            settings.selectAthenaLogFolder = document.getElementById(selectAthenaLogFolderId);
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