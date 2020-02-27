/*
    base Model is responsible for common settings
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
    function (n){
        //private data memebers
        var settings = {
            metadata: [],
            backup: [],
            mode: "New"
        }

        //Model interfaces functions
        Rigsarkiv.Hybris.Base = {
            initialize: function () {

            },
            callback: function () {
                return { 
                    mode: settings.mode,
                    metadata: settings.metadata,
                    backup: settings.backup,
                    setMode: function(mode) {
                        settings.mode = mode;   
                    },
                    setMetadata: function(metadata) {
                        settings.metadata = metadata;   
                    },
                    setBackup: function(backup) {
                        settings.backup = backup;   
                    }
                }
            }
        }
    }(jQuery);    
}(jQuery);