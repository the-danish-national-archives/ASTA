window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const {ipcRenderer} = require('electron')

    var settings = {
        selectStatisticsFileBtn: null,
        pathStatisticsFileTxt: null,
        selectedStatisticsFilePath: null
    }

    var AddEvents = function () {
        settings.selectStatisticsFileBtn.addEventListener('click', (event) => {
           ipcRenderer.send('dataextraction-open-file-dialog');
        })
        ipcRenderer.on('dataextraction-selected-statistics-file', (event, path) => {
            settings.selectedStatisticsFilePath = path; 
            console.log(`selected path: ${path}`); 
            settings.pathStatisticsFileTxt.value = settings.selectedStatisticsFilePath;
         })
    }
    Rigsarkiv.DataExtraction = {        
        initialize: function (selectStatisticsFileId,pathStatisticsFileId) {
            settings.selectStatisticsFileBtn = document.getElementById(selectStatisticsFileId);
            settings.pathStatisticsFileTxt = document.getElementById(pathStatisticsFileId);
            AddEvents();
        }
    };
}(jQuery);