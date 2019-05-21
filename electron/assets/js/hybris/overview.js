/*
    Model is responsible for overview
    initialize interface inputs: elements from <div id="hybris-panel-overview">
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
    function (n) {
        const { ipcRenderer } = require('electron');

         //private data memebers
         var settings = {
            structureCallback: null
         }

        //Model interfaces functions
        Rigsarkiv.Hybris.Overview = {
            initialize: function (structureCallback) {
                settings.structureCallback = structureCallback;
            }
        }
    }(jQuery);
}(jQuery);