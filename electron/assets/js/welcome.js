/*
    Model is responsible for rights (admin, none admin)
    initialize interface inputs: 
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const fs = require('fs');
    const path = require('path');
    //const os = require('os');

    //private data members
    var settings = {
        footerNumberingParagraph: null,
        versionFileName: "version.json",
        scriptPath: "./assets/scripts/{0}",
        resourceWinPath: "resources\\{0}",
        footerNumbering: null,
        version: null,
        year: null
    }

    //Model interfaces functions
    Rigsarkiv.Version = {
        initialize: function (footerNumbering) {
            settings.footerNumberingParagraph = document.getElementById(footerNumbering);
            var versionFilePath = settings.scriptPath.format(settings.versionFileName); 
            settings.footerNumbering = JSON.parse(fs.readFileSync(versionFilePath));
            settings.version = settings.footerNumbering.versionNo;
            settings.year = settings.footerNumbering.publishYear;
            var tempText = '&copy; {0} <b>ASTA - Aflevering af Statistikfiler Til Arkiv</b> - Version: {1}'.format(settings.year,settings.version)
            document.getElementById("versionfooter").innerHTML = tempText
        }

    }
}(jQuery);