window.Rigsarkiv = window.Rigsarkiv || {},
    function (n) {

        const { ipcRenderer } = require('electron');
        const fs = require('fs');

        const startNumberPattern = /^([a-zA-ZæøåÆØÅ])([a-zA-ZæøåÆØÅ0-9]*)$/;
        const quotesPattern = /^"([a-zA-ZæøåÆØÅ0-9]*)"$/;
        const spacePattern = /\s/;
        const strLength = 128;

        var settings = {
            fileName: null,
            fileDescr: null,
            keyVar: null,
            foreignFileName: null,
            foreignKeyVarName: null,
            foreignFileRefVar: null,
            okBtn: null,
            // Messages
            fileNameReq: null,
            numberFirst: null,
            illegalChar: null,
            //var
            contents: ["","","",""],
            isValidMetadata: true,
            metadataFileName: "{0}.txt",
            dataFileName: "{0}.csv",
            metadataFilePath: "./assets/scripts/metadata.txt"
        }

        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            //settings.outputStatisticsErrorSpn.hidden = false;
            //settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format(err.message);
        }

        var Reset = function () {
            settings.isValidMetadata = true;
            settings.fileNameReq.hidden = true;
            settings.numberFirst.hidden = true;
            settings.contents = ["","","",""];
        }

        var ValidateFields = function() {
            if (settings.fileName.value === "") {
                console.log('empty input');
                settings.isValidMetadata = false;
            }
            if (spacePattern.test(settings.fileName.value)) {
                console.log('error');
                settings.isValidMetadata = false;
            }
            if (startNumberPattern.test(settings.fileName.value)) {
                console.log("valid");
            } else {                
                console.log('failed')
                settings.isValidMetadata = false;
            }
            if (quotesPattern.test(settings.fileName.value)){
                console.log('valid quotes');
            }
            if (settings.fileName.value.length > strLength) {
                console.log('input too long');
                settings.isValidMetadata = false;
            }
        }

        var GetMetaDataFileName = function() {
            return settings.metadataFileName.format(GetDataFolderName());
        }

        var GetDataFolderName = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            var folders = dataFolderPath.split("/");
            return folders[folders.length - 1];
        }

        var Cleanup = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            fs.readdir(dataFolderPath, (err, files) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    var fileName = GetDataFolderName();
                    var dataFolderPath = settings.extractionCallback().dataFolderPath;
                    files.forEach(file => {
                        if(file != settings.dataFileName.format(fileName) && file != settings.metadataFileName.format(fileName)) {
                            console.log("delete file : " + file);
                            fs.unlinkSync("{0}/{1}".format(dataFolderPath,file));
                        }
                    });
                    //settings.outputStatisticsOkCopyScriptSpn.innerHTML = settings.outputStatisticsOkCopyScriptText.format(settings.scriptType,scriptFileName,GetFileName());
                    //settings.okScriptDataPath.innerHTML = GetLocalFolderPath();                        
                }
            });
        }

        var RenameFile = function() {
            var folders = settings.extractionCallback().selectedStatisticsFilePath.normlizePath().split("/");
            var srcFileName = folders[folders.length - 1];
            srcFileName = srcFileName.substring(0,srcFileName.indexOf("."));
            var destFileName = GetDataFolderName();
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            console.log("rename " + srcFileName + " file to: {0}.csv".format(destFileName));
            fs.rename("{0}/{1}".format(dataFolderPath,settings.dataFileName.format(srcFileName)) ,"{0}/{1}".format(dataFolderPath,settings.dataFileName.format(destFileName)) , (err) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    console.log('Rename complete!');
                    Cleanup();
                }
              }); 
        }

        var UpdateFile = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            var metadataFileName = GetMetaDataFileName();
            fs.readFile("{0}/{1}".format(dataFolderPath,metadataFileName), (err, data) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    var callback = settings.extractionCallback();
                    var dataFolderPath = callback.dataFolderPath;
                    var metadataFileName = GetMetaDataFileName();
                    var scriptType = callback.scriptType;
                    var updatedData = data.toString().format(scriptType,settings.fileName.value,settings.fileDescr.value,settings.keyVar.value,settings.foreignFileName.value,settings.foreignKeyVarName.value,settings.foreignFileRefVar.value,settings.contents[0],settings.contents[1],settings.contents[2],settings.contents[3]);
                    fs.writeFile("{0}/{1}".format(dataFolderPath,metadataFileName), updatedData, (err) => {
                        if (err) {
                            HandleError(err);
                        }
                        else {
                            RenameFile();
                        }
                    });
                }
            });
        }

        var EnsureFile = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            var metadataFileName = GetMetaDataFileName();
            console.log(`copy ${metadataFileName} file to: ${dataFolderPath}`);
            fs.copyFile(settings.metadataFilePath, "{0}/{1}".format(dataFolderPath,metadataFileName), (err) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    var dataFolderPath = settings.extractionCallback().dataFolderPath;
                    var metadataFileName = GetMetaDataFileName();
                    console.log(metadataFileName + ' was copied to '+ dataFolderPath);
                    UpdateFile();
                }
            });
        }

        var EnsureData = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            fs.readdir(dataFolderPath, (err, files) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    var dataFolderPath = settings.extractionCallback().dataFolderPath;
                    console.log(`get texts contents: ${dataFolderPath}`);
                    files.forEach(file => {
                        var filePath = "{0}/{1}".format(dataFolderPath,file);
                        if(file.lastIndexOf("_VARIABEL.txt") > -1) {
                            settings.contents[0] = fs.readFileSync(filePath).toString();
                        }
                        if(file.lastIndexOf("_VARIABELBESKRIVELSE.txt") > -1) {
                            settings.contents[1] = fs.readFileSync(filePath).toString();
                        }
                        if(file.lastIndexOf("_KODELISTE.txt") > -1) {
                            settings.contents[2] = fs.readFileSync(filePath).toString();
                        }
                        if(file.lastIndexOf("_BRUGERKODE.txt") > -1) {
                            settings.contents[3] = fs.readFileSync(filePath).toString();
                        }
                    });
                    EnsureFile();
                }
            });
        }

        var AddEvents = function () {
            settings.okBtn.addEventListener('click', function (event) {
                Reset();
                ValidateFields();
                if(settings.isValidMetadata) { EnsureData(); }
            })
        }

        Rigsarkiv.MetaData = {
            initialize: function (extractionCallback,metadataFileName, metadataFileNameDescription, metadataKeyVariable, metadataForeignFileName, metadataForeignKeyVariableName, metadataReferenceVariable, metdataOkBtn, inputFileNameRequired, inputNumberFirst, inputIllegalChar) {
                settings.extractionCallback = extractionCallback;
                settings.fileName = document.getElementById(metadataFileName);
                settings.fileDescr = document.getElementById(metadataFileNameDescription);
                settings.keyVar = document.getElementById(metadataKeyVariable);
                settings.foreignFileName = document.getElementById(metadataForeignFileName);
                settings.foreignKeyVarName = document.getElementById(metadataForeignKeyVariableName);
                settings.foreignFileRefVar = document.getElementById(metadataReferenceVariable);
                settings.okBtn = document.getElementById(metdataOkBtn);
                settings.fileNameReq = document.getElementById(inputFileNameRequired);
                settings.numberFirst = document.getElementById(inputNumberFirst);
                settings.illegalChar = document.getElementById(inputIllegalChar);
                AddEvents();
            }
        }
    }(jQuery);