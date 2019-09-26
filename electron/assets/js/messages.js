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
                    var title = Rigsarkiv.Language.callback().getValue(elementId + "-Title"); 
                    //TODO : remove
                    if(title == null) { title = document.getElementById(elementId + "-Title").innerHTML; }                    
                    var text = Rigsarkiv.Language.callback().getValue(elementId + "-Title"); 
                    //TODO : remove
                    if(text == null) { text = document.getElementById(elementId + "-Text").innerHTML; }                   
                    element.addEventListener('click', (event) => {
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