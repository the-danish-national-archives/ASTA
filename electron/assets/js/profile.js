/*
    Model is responsible for user profile
    initialize interface inputs: 
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    const electron = require('electron');
    const fs = require('fs');
    
    //private data members
    var settings = {
        outputErrorSpn: null,
        outputErrorText: null,
        menuSection: null,
        languagesDropdown: null,
        linkIds: [],
        defaultLcid: "da-DK",
        fileName: "profile.json",
        data: { "lcid":"" }
    }

    //get profile path
    var GetFilePath = function() {
        var userDataPath = electron.remote["app"].getPath('userData');
        return (userDataPath.indexOf("\\") > -1) ? "{0}\\{1}".format(userDataPath,settings.fileName) : "{0}/{1}".format(userDataPath,settings.fileName);        
    }

    // ensure profile json file
    var EnsureProfile = function() {        
        var filePath = GetFilePath();
        try
        {
            if(!fs.existsSync(filePath)) {
                console.logInfo(`create profile file: ${filePath}`,"Rigsarkiv.Profile.EnsureProfile");
                settings.data.lcid = settings.defaultLcid;
                fs.writeFileSync(filePath, JSON.stringify(settings.data));
            }
            settings.data = JSON.parse(fs.readFileSync(filePath));
        }
        catch(err) 
        {
            err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Profile.EnsureProfile"); 
        }
    }

    //add Event Listener to HTML elmenets
    var AddEvents = function () {
        var link = null;
        settings.linkIds.forEach(linkId => {
            link = document.getElementById(linkId);
            if(link != null) {
                link.addEventListener('click', (event) => {
                    settings.menuSection.click();
                });
            }
            else {
                console.logInfo(`none exist elment with id: ${linkId}`,"Rigsarkiv.Profile.AddEvents");
            }                
        });
        settings.saveBtn.addEventListener('click', function (event) {
            settings.data.lcid = settings.languagesDropdown.options[settings.languagesDropdown.selectedIndex].value; 
            var filePath = GetFilePath();
            console.logInfo(`update profile file: ${filePath}`,"Rigsarkiv.Profile.EnsureProfile");
            try
            {
                fs.writeFileSync(filePath, JSON.stringify(settings.data));
            }
            catch(err) 
            {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Profile.AddEvents"); 
            }
            Rigsarkiv.Language.callback().setLanguage(settings.data.lcid);
        });
    }

    //Model interfaces functions
    Rigsarkiv.Profile = {
        initialize: function (outputErrorId,menuId,languagesId,saveId,linkIds) {
            EnsureProfile();           
            settings.outputErrorSpn = document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML;
            settings.menuSection = document.getElementById(menuId);
            settings.languagesDropdown = document.getElementById(languagesId);
            settings.languagesDropdown.value = settings.data.lcid;
            settings.saveBtn = document.getElementById(saveId);
            settings.linkIds = linkIds;
            AddEvents();           
        },
        callback: function () {
            return { 
                data: settings.data
            }
        } 
    }
}(jQuery);