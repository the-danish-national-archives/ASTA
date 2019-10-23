/*
    Model is responsible for language translator   
    alle languages are saved at ./assets/languages/
*/
window.Rigsarkiv = window.Rigsarkiv || {},
    function (n) {
        const fs = require('fs');
        const path = require('path');
        const os = require('os');        

        //private data memebers
        var settings = {
            outputErrorSpn: null,
            outputErrorText: null,
            lcid: null,
            keys: [],
            values: [],
            resourceWinPath: "resources\\{0}.json",
            documentPath: "./assets/languages/{0}.json"
        }

        //get value by key
        var getValue = function(key) {
            var result = null;
            if(settings.keys.includes(key)) {
                result = settings.values[settings.keys.indexOf(key)];
            }
            else {
                console.logInfo(`key not exists ${key}`,"Rigsarkiv.Language.getValue");
            }
            return result;
        }

        // Ensure version Data
        var EnsureData = function() {
            var languageFilePath = settings.documentPath.format(settings.lcid);        
            if(!fs.existsSync(languageFilePath)) {
                var rootPath = null;
                if(os.platform() == "win32") {
                    rootPath = path.join('./');
                    languageFilePath = path.join(rootPath,settings.resourceWinPath.format(settings.lcid));
                }
                if(os.platform() == "darwin") {
                    var folders =  __dirname.split("/");
                    rootPath = folders.slice(0,folders.length - 3).join("/");
                    languageFilePath = "{0}/{1}.json".format(rootPath,settings.lcid);
                }
            }        
            console.logInfo(`read ${settings.lcid} file from: ${languageFilePath}`,"Rigsarkiv.Language.EnsureData");
            return JSON.parse(fs.readFileSync(languageFilePath));            
        }

        // Ensure Caches
        var EnsureCache = function(data) {
            settings.keys = [];
            settings.values = [];
            for(var i =0; i < data.length; i++) {
                console.logInfo(`add section ${data[i].section}`,"Rigsarkiv.Language.EnsureCache");
                data[i].keys.forEach(pair => {
                  settings.keys.push(pair.key);
                  settings.values.push(pair.value);  
                });
            }
        }

        // Update language
        var Update = function(lcid) {
            settings.lcid = lcid;            
            try
            {
                var data = EnsureData();
                EnsureCache(data);
                console.logInfo(`apply langaugaes keys`,"Rigsarkiv.Language.EnsureData");
                $(".languages").each(function() {
                    var id = this.id;
                    if(id !== undefined) {
                        var tagName = $(this).get(0).tagName;
                        if(tagName === "INPUT" || tagName === "TEXTAREA") {
                            this.placeholder = getValue(id);
                        }
                        else {
                            this.innerHTML = getValue(id);
                        }                        
                    }                        
                });
            }
            catch(err) 
            {
                err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Language.Update"); 
            }            
        }

        Rigsarkiv.Language = {
            initialize: function (outputErrorId) {
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML; 
            },
            callback: function () {
                return { 
                    lcid: settings.lcid,
                    setLanguage: function(lcid) {
                        Update(lcid);   
                    },
                    getValue: function(key) {
                        return getValue(key);   
                    }
                }
            } 
        }
}(jQuery);