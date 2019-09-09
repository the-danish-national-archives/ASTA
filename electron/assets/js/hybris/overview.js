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
            selectDeliveryPackage: null,
            validateBtn: null,
            menuSection: null,
            nemesisCallback: null
         }

         //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.validateBtn.addEventListener('click', (event) => {
                Rigsarkiv.Nemesis.Structure.callback().update(Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath);
                settings.menuSection.click();
            });
            settings.selectDeliveryPackage.addEventListener('click', (event) => {
                var folderPath = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath;
                ipcRenderer.send('open-item',folderPath);
            });            
        }

        //Model interfaces functions
        Rigsarkiv.Hybris.Overview = {
            initialize: function (selectDeliveryPackageId,validateId,menuId) {
                settings.selectDeliveryPackage = document.getElementById(selectDeliveryPackageId);
                settings.validateBtn = document.getElementById(validateId);
                settings.menuSection = document.getElementById(menuId);
                AddEvents();
            }
        }
    }(jQuery);
}(jQuery);