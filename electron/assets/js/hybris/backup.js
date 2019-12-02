/*
    Model is responsible for Ensure Delivery Package backups
    initialize interface inputs: elements from <div id="hybris-panel-backup">
    callback interface outputs:  delivery Package backup folder
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
    function (n){
        const {ipcRenderer} = require('electron')
        const fs = require('fs');
        const fse = require('fs-extra');

        //private data memebers
        var settings = {
            outputErrorSpn: null,
            outputErrorText: null, 
            foldersTbl: null,
            okBtn: null,
            printBtn: null,
            cancelBtn: null,
            spinnerId: null,
            spinnerClass: null,
            indexfilesTab: null,
            hasSelected: false,
            checkboxPrefixId: "hybris-backup-folder-",
            pathPostfix: "{0}ASTA_udtrækslog_{1}",
        }

        //reset status & input fields
        var Reset = function () {
            settings.hasSelected = false;
            settings.outputErrorSpn.hidden = true;
            $("#{0} tr:not(:first-child)".format(settings.foldersTbl.id)).remove();
       }

        // Render backups folders control
        var RenderFolders = function() {
            var folders = Rigsarkiv.Hybris.Base.callback().backup;
            if(folders != null && folders.length > 0) {
                var index = 0;
                folders.forEach(folder => {
                    $(settings.foldersTbl).append("<tr><td><input type=\"checkbox\" id=\"{0}{1}\" value=\"{1}\" checked/></td><td>{2}</td><td>{3}</td><td>{4}</td></tr>".format(settings.checkboxPrefixId,index,folder.path,folder.name,folder.size));
                    index = index + 1;
                });
            }
        }

        //ensure output directory
        var EnsurePath = function() {
            try 
            {
                var selectedFolderPath = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath;
                var folders = selectedFolderPath.getFolders();
                var fileName = folders[folders.length - 1];
                settings.filePath = settings.pathPostfix.format(selectedFolderPath.substring(0,selectedFolderPath.lastIndexOf((selectedFolderPath.indexOf("\\") > -1) ? "\\" : "/") + 1),fileName);
                if(!fs.existsSync(settings.filePath)) {                        
                    console.logInfo(`Create log folder: ${settings.filePath}`,"Rigsarkiv.Hybris.Backup.EnsurePath");                     
                    fs.mkdirSync(settings.filePath);
                }
            }
            catch(err) {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.Backup.EnsurePath");
            }
        }

        //copy backups
        var EnsureData = function() {
            EnsurePath();
            try 
            {
                var folders = Rigsarkiv.Hybris.Base.callback().backup;
                $("input:checkbox[id^=" + settings.checkboxPrefixId + "]").each(function(index) {
                    if(this.checked) {
                        var folder = folders[index];
                        console.logInfo(`backup folder: ${folder.path}`,"Rigsarkiv.Hybris.Backup.EnsureData");
                        var destPath = settings.filePath.indexOf("\\") > -1 ? "{0}\\{1}".format(settings.filePath,folder.name) : "{0}/{1}".format(settings.filePath,folder.name);
                        fse.copySync(folder.path,destPath); 
                    }
                  });
            }
            catch(err) {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.Backup.EnsureData");
            } 
        }

        //enable/diable waiting spinner
        var UpdateSpinner = function(spinnerClass) {
            var disabled = (spinnerClass === "") ? false : true;
            settings.spinner.className = spinnerClass;
            settings.printBtn.disabled = disabled;
            settings.okBtn.disabled = disabled;
        }

         //add Event Listener to HTML elmenets
         var AddEvents = function () {
            settings.okBtn.addEventListener('click', function (event) {                
                $("input:checkbox[id^=" + settings.checkboxPrefixId + "]").each(function(index) {
                    if(this.checked) {  settings.hasSelected = true; }
                }); 
                if(!settings.hasSelected) {
                    ipcRenderer.send('open-confirm-dialog','backup',Rigsarkiv.Language.callback().getValue("hybris-output-backup-NextConfirm-Title"),Rigsarkiv.Language.callback().getValue("hybris-output-backup-NextConfirm-Text"),Rigsarkiv.Language.callback().getValue("hybris-output-backup-OkConfirm"),Rigsarkiv.Language.callback().getValue("hybris-output-backup-CancelConfirm"));
                }
                else {             
                    UpdateSpinner(settings.spinnerClass); 
                    EnsureData();
                    UpdateSpinner("");
                    settings.indexfilesTab.click();
                }
            });
            ipcRenderer.on('confirm-dialog-selection-backup', (event, index) => {
                if(index === 0) {
                    settings.indexfilesTab.click();
                }            
            });
            settings.cancelBtn.addEventListener('click', function (event) {
                $("input:checkbox[id^=" + settings.checkboxPrefixId + "]").each(function(index) {
                    this.checked = false; 
                }); 
            });
            settings.printBtn.addEventListener('click', function (event) {
                EnsurePath();
            });
         }

        //Model interfaces functions
        Rigsarkiv.Hybris.Backup = {
            initialize: function (outputErrorId,foldersId,okId,printId,cancelId,spinnerId,indexfilesTabId) {
                settings.outputErrorSpn =  document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.foldersTbl = document.getElementById(foldersId);
                settings.okBtn = document.getElementById(okId);
                settings.printBtn = document.getElementById(printId);
                settings.cancelBtn = document.getElementById(cancelId);
                settings.spinner = document.getElementById(spinnerId);
                settings.spinnerClass = settings.spinner.className;
                settings.spinner.className = "";
                settings.indexfilesTab = document.getElementById(indexfilesTabId);
                AddEvents();
            },
                callback: function () {
                    return { 
                        reset: function()  { 
                            Rigsarkiv.Hybris.Base.callback().setBackup([]);
                            Reset();
                        },
                        load: function() {
                            try 
                            {
                                Reset();
                                RenderFolders();
                            }
                            catch(err) {
                                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.Backup.callback.load");
                            } 
                        } 
                    };
                }
        }
    }(jQuery);    
}(jQuery);