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

        //private data memebers
        var settings = { 

        }

        //Model interfaces functions
        Rigsarkiv.Hybris.Backup = {
            initialize: function () {

            },
                callback: function () {
                    return { 
                        reset: function() 
                        { 
                            Rigsarkiv.Hybris.Base.callback().setBackup([]);
                        } 
                    };
                }
        }
    }(jQuery);    
}(jQuery);