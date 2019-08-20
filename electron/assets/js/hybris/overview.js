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
            structureCallback: null,
            selectDeliveryPackage: null,
            validateBtn: null,
            menuSection: null,
            nemesisCallback: null
         }

         //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.validateBtn.addEventListener('click', (event) => {
                settings.nemesisCallback().update(settings.structureCallback().deliveryPackagePath);
                settings.menuSection.click();
            });
            settings.selectDeliveryPackage.addEventListener('click', (event) => {
                var folderPath = settings.structureCallback().deliveryPackagePath;
                ipcRenderer.send('open-item',folderPath);
            });            
        }

        //Model interfaces functions
        Rigsarkiv.Hybris.Overview = {
            initialize: function (structureCallback,nemesisCallback,selectDeliveryPackageId,validateId,menuId) {
                settings.structureCallback = structureCallback;
                settings.selectDeliveryPackage = document.getElementById(selectDeliveryPackageId);
                settings.validateBtn = document.getElementById(validateId);
                settings.menuSection = document.getElementById(menuId);
                settings.nemesisCallback = nemesisCallback;
                AddEvents();
            }
        }
    }(jQuery);
}(jQuery);