/*
    Model is responsible for Ensure Delivery Package backups
    initialize interface inputs: elements from <div id="hybris-panel-backup">
    callback interface outputs:  delivery Package backup folder
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
    function (n){
        const {ipcRenderer} = require('electron');
        const {shell} = require('electron');
        const path = require('path');
        const os = require('os');
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
            filePostfix: "{0}_ASTA_backup.html",
            pathPostfix: "{0}ASTA_udtrækslog_{1}",
            templateFileName: "backup.html",
            scriptPath: "./assets/scripts/{0}",
            resourceWinPath: "resources\\{0}",
            filePath: null,
            folderPath: null,
            logs: []
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
                var folderIndex = 0;
                folders.forEach(folder => {
                    var filesHtml = "";
                    var index = 0;
                    if(folder.files.length > 0) {
                        folder.files.forEach(file => {
                            filesHtml += "<input type=\"checkbox\" id=\"{0}_{1}_{2}\" value=\"{1}_{2}\" checked>{3}</input><br/>".format(settings.checkboxPrefixId,folderIndex,index,file);
                            index = index + 1;
                        });
                        $(settings.foldersTbl).append("<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>".format(filesHtml,folder.path,folder.name,folder.size));
                    }
                    folderIndex = folderIndex + 1;
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
                settings.folderPath = settings.pathPostfix.format(selectedFolderPath.substring(0,selectedFolderPath.lastIndexOf((selectedFolderPath.indexOf("\\") > -1) ? "\\" : "/") + 1),fileName);
                if(!fs.existsSync(settings.folderPath)) {                        
                    console.logInfo(`Create log folder: ${settings.folderPath}`,"Rigsarkiv.Hybris.Backup.EnsurePath");                     
                    fs.mkdirSync(settings.folderPath);
                }
            }
            catch(err) {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.Backup.EnsurePath");
            }
        }

        //get checked files
        var GetSelectedData = function() {
            var keys = [];
            var values = [];
            $("input:checkbox[id^=" + settings.checkboxPrefixId + "]").each(function(index) {
                if(this.checked) {
                    var pair = this.value.split("_");
                    var folderIndex = parseInt(pair[0]);  
                    var fileIndex = parseInt(pair[1]);
                    var folder = Rigsarkiv.Hybris.Base.callback().backup[folderIndex];
                    if(!keys.includes(folder.name)) {
                        keys.push(folder.name);
                        values.push([]);
                    }
                    values[keys.indexOf(folder.name)].push(folder.files[fileIndex]);
                 }
            });
            return { "keys":keys, "values":values }
        }

        //copy backups
        var EnsureStructure = function() {
            EnsurePath();
            try 
            {
                var selectedData = GetSelectedData();
                Rigsarkiv.Hybris.Base.callback().backup.forEach(folder => {
                    if(selectedData.keys.includes(folder.name)) {
                        var destPath = settings.folderPath.indexOf("\\") > -1 ? "{0}\\{1}".format(settings.folderPath,folder.name) : "{0}/{1}".format(settings.folderPath,folder.name);                        
                        if(!fs.existsSync(destPath)) {                        
                            console.logInfo(`Create folder: ${destPath}`,"Rigsarkiv.Hybris.Backup.EnsurePath");                     
                            fs.mkdirSync(destPath);
                        }
                        selectedData.values[selectedData.keys.indexOf(folder.name)].forEach(file => {
                            var srcFilePath = settings.folderPath.indexOf("\\") > -1 ? "{0}\\{1}".format(folder.path,file) : "{0}/{1}".format(folder.path,file);
                            var destFilePath = settings.folderPath.indexOf("\\") > -1 ? "{0}\\{1}".format(destPath,file) : "{0}/{1}".format(destPath,file);
                            console.logInfo(`backup file: ${srcFilePath}`,"Rigsarkiv.Hybris.Backup.EnsureStructure");
                            fs.copyFileSync(srcFilePath, destFilePath);
                        });
                    }
                });
            }
            catch(err) {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.Backup.EnsureStructure");
            } 
        }

        //commit print data
        var EnsureData = function() {
            var data = fs.readFileSync(settings.filePath);        
            var folders = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath.getFolders();
            var folderName = folders[folders.length - 1];
            var title = Rigsarkiv.Language.callback().getValue("hybris-backup-template-Title");
            var foldersList = Rigsarkiv.Language.callback().getValue("hybris-backup-template-foldersList-H3").format(folderName);
            var th1 = Rigsarkiv.Language.callback().getValue("hybris-backup-template-foldersList-TH1");
            var th2 = Rigsarkiv.Language.callback().getValue("hybris-backup-template-foldersList-TH2");
            var th3 = Rigsarkiv.Language.callback().getValue("hybris-backup-template-foldersList-TH3");
            var updatedData = data.toString().format(folderName,settings.logs.join("\r\n"),title,foldersList,th1,th2,th3);
            fs.writeFileSync(settings.filePath, updatedData);                         
            settings.logs = [];                               
        }

        //copy HTML template file to parent folder of selected Delivery Package folder
        var CopyFile = function() {
            var filePath = settings.scriptPath.format(settings.templateFileName);        
            if(!fs.existsSync(filePath)) {
                var rootPath = null;
                if(os.platform() == "win32") {
                    rootPath = path.join('./');
                    filePath = path.join(rootPath,settings.resourceWinPath.format(settings.templateFileName));
                }
                if(os.platform() == "darwin") {
                    var folders =  __dirname.split("/");
                    rootPath = folders.slice(0,folders.length - 3).join("/");
                    filePath = "{0}/{1}".format(rootPath,settings.templateFileName);
                }
            }        
            console.logInfo(`copy ${settings.templateFileName} file to: ${settings.filePath}`,"Rigsarkiv.Hybris.Backup.CopyFile");
            fs.copyFileSync(filePath, settings.filePath);
            EnsureData();        
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
                    EnsureStructure();
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
                var folders = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath.getFolders();
                var fileName = folders[folders.length - 1];
                settings.filePath = settings.folderPath.indexOf("\\") > -1 ? "{0}\\{1}".format(settings.folderPath,settings.filePostfix.format(fileName)) : "{0}/{1}".format(settings.folderPath,settings.filePostfix.format(fileName));
                
                var selectedData = GetSelectedData();
                Rigsarkiv.Hybris.Base.callback().backup.forEach(folder => {
                    if(selectedData.keys.includes(folder.name)) {                        
                        settings.logs.push("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>".format(selectedData.values[selectedData.keys.indexOf(folder.name)].join("<br/>"),folder.path,folder.name));
                    }
                });
                CopyFile();
                shell.openItem(settings.filePath);
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