/*
    Model is responsible for Validate Metadata tables (Flow 2.0)
    initialize interface inputs: elements from <div class="formularContainer">
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Nemesis = Rigsarkiv.Nemesis || {},
    function (n) {
        const {ipcRenderer} = require('electron');
        const fs = require('fs');

        //private data memebers
        var settings = { 
            selectDirBtn: null,
            pathDirTxt: null,
            selectedPath: null,
            validateBtn: null,
            outputPrefix: null,
            logCallback: null,
            logStartSpn: null,
            logEndNoErrorSpn: null,
            logEndWithErrorSpn:null,
            outputText: {},
            logType: "metadata"
        }

        //reset status & input fields
        var Reset = function () {
            $("span[id^='" + settings.outputPrefix + "']").hide();
             $("span[id^='" + settings.outputPrefix + "']").each(function() {
                $(this).html(settings.outputText[this.id]);
            });
        }

        //output system error messages
        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputErrorSpn.hidden = false;
            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);
        }

        // get selected folder name 
        var GetFolderName = function() {
            var folders = settings.selectedPath[0].getFolders();
            return folders[folders.length - 1];
        }

        //write log file
        var CommitLog = function (hasError) {
            var folderName = GetFolderName();
            if(!hasError) {
                settings.logCallback().section(settings.logType,folderName,settings.logEndNoErrorSpn.innerHTML);
            } else {
                settings.logCallback().section(settings.logType,folderName,settings.logEndWithErrorSpn.innerHTML);
            }
            settings.logCallback().commit(settings.selectedPath[0]); 
        }

        var ValidateText = function () {
            console.log(`metadata selected path: ${settings.selectedPath}`); 
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.validateBtn.addEventListener('click', (event) => {
                Reset();
                if(settings.selectedPath == null || settings.pathDirTxt.value === "") { return; }
                ValidateText();
            })
            settings.selectDirBtn.addEventListener('click', (event) => {
                ipcRenderer.send('validation-open-file-dialog');
            })
            ipcRenderer.on('validation-selected-directory', (event, path) => {
                settings.selectedPath = path; 
                console.log(`selected path: ${path}`); 
                settings.pathDirTxt.value = settings.selectedPath;
            })            
        }

        //Model interfaces functions
        Rigsarkiv.Nemesis.MetaData = {        
            initialize: function (logCallback,outputErrorId,selectDirectoryId,pathDirectoryId,validateId,logStartId,logEndNoErrorId,logEndWithErrorId,outputPrefix) {            
                settings.logCallback = logCallback;
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.selectDirBtn = document.getElementById(selectDirectoryId);
                settings.pathDirTxt = document.getElementById(pathDirectoryId);
                settings.validateBtn = document.getElementById(validateId);
                settings.logStartSpn = document.getElementById(logStartId);
                settings.logEndNoErrorSpn = document.getElementById(logEndNoErrorId);  
                settings.logEndWithErrorSpn = document.getElementById(logEndWithErrorId);
                settings.outputPrefix = outputPrefix;
                $("span[id^='" + settings.outputPrefix + "']").each(function() {
                    settings.outputText[this.id] = $(this).html();
                });
                //AddEvents();
            },
            callback: function () {
                return { 
                    validate: function(path) 
                    { 
                        settings.selectedPath = path;
                        ValidateText();
                    }  
                };
            }
        };
    }(jQuery);
}(jQuery);