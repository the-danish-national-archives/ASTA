/*
    base Model is responsible for common settings
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
    function (n){
        //private data memebers
        var settings = {
            mode: "New"
        }

        //Model interfaces functions
        Rigsarkiv.Hybris.Base = {
            initialize: function () {

            },
            callback: function () {
                return { 
                    mode: settings.mode,
                    setMode: function(mode) {
                        settings.mode = mode;   
                    }
                }
            }
        }
    }(jQuery);    
}(jQuery);