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
                    var titleElement = document.getElementById(elementId + "-Title");
                    var textElement = document.getElementById(elementId + "-Text");
                    element.addEventListener('click', (event) => {
                        ipcRenderer.send('open-information-dialog',titleElement.innerHTML,textElement.innerHTML);
                    });
                }
                else {
                    console.log(`none exist elment with id: ${elementId}`);
                }                
            });
        }
    };
}(jQuery);