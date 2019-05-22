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
            selectDeliveryPackageLink: null
         }

         //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.selectDeliveryPackage.addEventListener('click', (event) => {
                var folderPath = settings.structureCallback().deliveryPackagePath;
                ipcRenderer.send('open-item',folderPath);
            });
            settings.selectDeliveryPackageLink.addEventListener('click', (event) => {
                var folderPath = settings.structureCallback().deliveryPackagePath;
                ipcRenderer.send('open-item',folderPath);
            });
        }

        //Model interfaces functions
        Rigsarkiv.Hybris.Overview = {
            initialize: function (structureCallback,selectDeliveryPackageId,selectDeliveryPackageLinkId) {
                settings.structureCallback = structureCallback;
                settings.selectDeliveryPackage = document.getElementById(selectDeliveryPackageId);
                settings.selectDeliveryPackageLink = document.getElementById(selectDeliveryPackageLinkId);
                AddEvents();
            }
        }
    }(jQuery);
}(jQuery);