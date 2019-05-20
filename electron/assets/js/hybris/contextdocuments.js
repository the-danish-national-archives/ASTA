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
        const domParser = require('xmldom');

        //private data memebers
        var settings = {
            structureCallback: null,
            okBtn: null,
            outputErrorSpn: null,
            outputErrorText: null,
            xmlDocuments: null
        }

         //output system error messages
         var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputErrorSpn.hidden = false;
            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);            
        }

        // Render Documents control
        var RenderDocuments = function(data) {
            if(data != null && data.toString() != null && data.toString() !== "") {
                settings.xmlDocuments = new domParser.DOMParser().parseFromString(data.toString());
                for(var i = 0; i < settings.xmlDocuments.documentElement.childNodes.length;i++) {
                    var node = settings.xmlDocuments.documentElement.childNodes[i];
                    if(node.nodeName === "document" && node.childNodes != null) {
                        
                    }
                }    
            }
            else {

            }
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.okBtn.addEventListener('click', function (event) {
            });
        }
        
        //Model interfaces functions
        Rigsarkiv.Hybris.ContextDocuments = {
            initialize: function (structureCallback,outputErrorId,okId) {
                settings.structureCallback = structureCallback;
                settings.okBtn = document.getElementById(okId);
                settings.outputErrorSpn =  document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                AddEvents();
            },
            callback: function () {
                return {
                    load: function(data) {
                        try {
                            RenderDocuments(data);
                        }
                        catch(err) {
                            HandleError(err);
                        } 
                    }
                }
            }
        }
    }(jQuery);
}(jQuery);