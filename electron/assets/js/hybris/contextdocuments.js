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
            uploadsTbl: null,
            documents: []
        }

         //output system error messages
         var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputErrorSpn.hidden = false;
            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);            
        }

        //reset status & input fields
        var Reset = function () {
            settings.outputErrorSpn.hidden = true;
            $("#{0} tr:not(:first-child)".format(settings.uploadsTbl.id)).remove();
            settings.documents = [];
        }

        // Render Documents control
        var RenderDocuments = function(data) {
            if(data != null && data.toString() != null && data.toString() !== "") {
                var doc = new domParser.DOMParser().parseFromString(data.toString());
                for(var i = 0; i < doc.documentElement.childNodes.length;i++) {
                    var node = doc.documentElement.childNodes[i];
                    if(node.nodeName === "document" && node.childNodes != null) {
                        var document = {"id":"", "title":"", "path":""};
                        for(var j = 0; j < node.childNodes.length;j++) {
                            if(node.childNodes[j].nodeName === "documentID") {
                                document.id = node.childNodes[j].firstChild.data;
                            }
                            if(node.childNodes[j].nodeName === "documentTitle") {
                                document.title = node.childNodes[j].firstChild.data;
                            }
                        }
                        settings.documents.push(document);                         
                    }
                }
                settings.documents.forEach(document => {
                    $(settings.uploadsTbl).append("<tr><td>{0}</td><td>{1}</td><td><input type=\"text\" id=\"hybris-contextdocuments-document-{0}\" class=\"path\" text=\"{2}\"/></td><td><button id=\"hybris-indexfiles-contextdocuments-selectFile-{0}\">Browse</button></td></tr>".format(document.id,document.title,document.path));
                });
                    
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
            initialize: function (structureCallback,outputErrorId,okId,uploadsId) {
                settings.structureCallback = structureCallback;
                settings.okBtn = document.getElementById(okId);
                settings.outputErrorSpn =  document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.uploadsTbl = document.getElementById(uploadsId);
                AddEvents();
            },
            callback: function () {
                return {
                    load: function(data) {
                        try {
                            Reset();
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