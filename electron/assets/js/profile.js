/*
    Model is responsible for user profile
    initialize interface inputs: 
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {

    //private data members
    var settings = {
        outputErrorSpn: null,
        outputErrorText: null,
        menuSection: null,
        languagesDropdown: null,
        lcid: null,
        linkIds: []
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
                console.logInfo(`none exist elment with id: ${linkId}`,"Rigsarkiv.Profile.initialize");
            }                
        });
        settings.saveBtn.addEventListener('click', function (event) {
            settings.lcid = settings.languagesDropdown.options[settings.languagesDropdown.selectedIndex].value; 
            Rigsarkiv.Language.callback().setLanguage(settings.lcid);
        });
    }

    //Model interfaces functions
    Rigsarkiv.Profile = {
        initialize: function (outputErrorId,menuId,languagesId,saveId,linkIds) {
            settings.lcid = "en-GB";            
            settings.outputErrorSpn = document.getElementById(outputErrorId);
            settings.outputErrorText = settings.outputErrorSpn.innerHTML;
            settings.menuSection = document.getElementById(menuId);
            settings.languagesDropdown = document.getElementById(languagesId);
            settings.languagesDropdown.value = settings.lcid;
            settings.saveBtn = document.getElementById(saveId);
            settings.linkIds = linkIds;
            AddEvents();           
        },
        callback: function () {
            return { 
                lcid: settings.lcid
            }
        } 
    }
}(jQuery);