/*
    Model is responsible for upload context documents
    initialize interface inputs: elements from <div id="hybris-panel-contextdocuments">
    output context documents at /ContextDocumentation
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
    function (n) {
        const { ipcRenderer } = require('electron');
        const fs = require('fs');

        //private data memebers
        var settings = {
            structureCallback: null,
            IndecesPath: null,
            IndecesPostfix: "Indices",
        }
        
        //Model interfaces functions
        Rigsarkiv.Hybris.ContextDocuments = {
            initialize: function (structureCallback) {
                settings.structureCallback = structureCallback;
            },
            callback: function () {
                return {
                    load: function() {
                        settings.IndecesPath = settings.structureCallback().deliveryPackagePath;
                        settings.IndecesPath += (settings.IndecesPath.indexOf("\\") > -1) ? "\\{0}".format(settings.IndecesPostfix) : "/{0}".format(settings.IndecesPostfix);
                    }
                }
            }
        }
    }(jQuery);
}(jQuery);