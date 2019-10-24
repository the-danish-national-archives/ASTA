/*
 Model is responsible for popup information messages
 inputs array of postprefixed <i> elemets 
 message title get related <span> element with postfixed -Title
 message text get related <span> element with postfixed -Text
*/
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron')

    Rigsarkiv.Messages = {        
        initialize: function (elemnetIds) {
            var element = null;
            elemnetIds.forEach(elementId => {
                element = document.getElementById(elementId);
                if(element != null) {                                      
                    element.addEventListener('click', (event) => {
                        var srcElementId = event.srcElement.id;
                        var title = Rigsarkiv.Language.callback().getValue(srcElementId + "-Title"); 
                        var text = Rigsarkiv.Language.callback().getValue(srcElementId + "-Text"); 
                        ipcRenderer.send('open-information-dialog',title,text);
                    });
                }
                else {
                    console.logInfo(`none exist elment with id: ${elementId}`,"Rigsarkiv.Messages.initialize");
                }                
            });
        }
    };
}(jQuery);