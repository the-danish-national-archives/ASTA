window.Rigsarkiv = window.Rigsarkiv || {},
    function (n) {

        const { ipcRenderer } = require('electron');
        const {shell} = require('electron');
        const fs = require('fs');

        const startNumberPattern = /^([0-9])([a-zA-ZæøåÆØÅ0-9_]*)$/;
        const validFileNamePattern = /^([a-zA-ZæøåÆØÅ])([a-zA-ZæøåÆØÅ0-9_]*)$/;
        const reservedWordPattern = /^(END|INSERT|INTO)$/i;
        const strLength = 128;

        var settings = {
            fileName: null,
            fileDescr: null,
            keyVar: null,
            foreignFileName: null,
            foreignKeyVarName: null,
            foreignFileRefVar: null,
            okBtn: null,
            outputOkSpn: null,
            outputOkText: null,
            okDataPath: null,
            outputErrorSpn: null,
            outputErrorText: null,
            outputNewExtractionSpn: null,
            outputNewExtractionText: null,
            newExtractionBtn: null,
            exitBtn: null,
            outputExitSpn: null,
            extractionTab: null,
            structureTab: null,
            fileNameReq: null,
            fileDescrReq: null,
            numberFirst: null,
            illegalChar: null,
            fileNameLength: null,
            fileNameReservedWord: null,
            contents: ["","","",""],
            isValidMetadata: true,
            metadataFileName: "{0}.txt",
            dataFileName: "{0}.csv",
            metadataFilePath: "./assets/scripts/metadata.txt"
        }

        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            settings.outputErrorSpn.hidden = false;
            settings.outputErrorSpn.innerHTML = settings.outputErrorText.format(err.message);
        }

        var Reset = function () {
            settings.isValidMetadata = true;
            settings.fileNameReq.hidden = true;
            settings.fileDescrReq.hidden = true;
            settings.numberFirst.hidden = true;
            settings.illegalChar.hidden = true;
            settings.fileNameLength.hidden = true;
            settings.fileNameReservedWord.hidden = true;
            settings.outputOkSpn.hidden = true;
            settings.okDataPath.hidden = true;
            settings.outputErrorSpn.hidden = true;
            settings.outputNewExtractionSpn.hidden = true;
            settings.newExtractionBtn.hidden = true;
            settings.exitBtn.hidden = true;
            settings.outputExitSpn.hidden = true;
            settings.contents = ["","","",""];
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
                    var callback = settings.extractionCallback();
                    var dataFolderPath = callback.dataFolderPath;
                    files.forEach(file => {
                        if(file != settings.dataFileName.format(fileName) && file != settings.metadataFileName.format(fileName)) {
                            console.log("delete file : " + file);
                            fs.unlinkSync("{0}/{1}".format(dataFolderPath,file));
                        }
                    });
                    var folders = callback.selectedStatisticsFilePath.normlizePath().split("/");
                    settings.outputOkSpn.hidden = false;
                    settings.okDataPath.hidden = false;
                    settings.outputNewExtractionSpn.hidden = false;
                    settings.newExtractionBtn.hidden = false; 
                    settings.outputExitSpn.hidden = false;  
                    settings.exitBtn.hidden = false;
                    settings.outputOkSpn.innerHTML = settings.outputOkText.format(folders[folders.length - 1]);
                    settings.okDataPath.innerHTML = callback.localFolderPath;
                    folders = dataFolderPath.split("/");
                    settings.outputNewExtractionSpn.innerHTML = settings.outputNewExtractionText.format(folders[folders.length - 3]);                                       
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
                    var foreignKeyVarName = (settings.foreignKeyVarName.value != null && settings.foreignKeyVarName.value !== "") ? "'{0}'".format(settings.foreignKeyVarName.value) : "";
                    var foreignFileRefVar = (settings.foreignFileRefVar.value != null && settings.foreignFileRefVar.value !== "") ? "'{0}'".format(settings.foreignFileRefVar.value) : "";
                    var updatedData = data.toString().format(scriptType,settings.fileName.value,settings.fileDescr.value,settings.keyVar.value,settings.foreignFileName.value,foreignKeyVarName,foreignFileRefVar,
                        settings.contents[0],settings.contents[1],settings.contents[2],settings.contents[3]);
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

        var ValidateFields = function() {
            if (settings.fileName.value === "") {
                settings.fileNameReq.hidden = false;
                settings.isValidMetadata = false;
            }
            if(settings.isValidMetadata && settings.fileDescr.value === "") {
                settings.fileDescrReq.hidden = false;
                settings.isValidMetadata = false;
            }
            if (settings.isValidMetadata && startNumberPattern.test(settings.fileName.value)) {
                settings.numberFirst.hidden = false;
                settings.isValidMetadata = false;
            }
            if (settings.isValidMetadata && !validFileNamePattern.test(settings.fileName.value)) {
                settings.illegalChar.hidden = false;
                settings.isValidMetadata = false;
            }
            if (settings.isValidMetadata && settings.fileName.value.length > strLength) {
                settings.fileNameLength.hidden = false;
                settings.isValidMetadata = false;
            }
            if (settings.isValidMetadata && reservedWordPattern.test(settings.fileName.value)) {
                settings.fileNameReservedWord.hidden = false;
                settings.isValidMetadata = false;
            }
        }

        var ResetExtraction = function() {
            settings.extractionCallback().reset();
            Reset();
            settings.fileName.value = "";
            settings.fileDescr.value = "";
            settings.keyVar.value = "";
            settings.foreignFileName.value = "";
            settings.foreignKeyVarName.value = "";
            settings.foreignFileRefVar.value = "";
        }

        var AddEvents = function () {
            settings.exitBtn.addEventListener('click', (event) => {
                settings.extractionCallback().structureCallback.reset();
                ResetExtraction();
                settings.structureTab.click();
            });
            settings.newExtractionBtn.addEventListener('click', (event) => {
                ResetExtraction();
                settings.extractionTab.click();
            });
            settings.okDataPath.addEventListener('click', (event) => {
                shell.openItem(settings.extractionCallback().localFolderPath);
            })
            settings.okBtn.addEventListener('click', function (event) {
                Reset();
                ValidateFields();
                if(settings.isValidMetadata) { EnsureData(); }
            })
        }

        Rigsarkiv.MetaData = {
            initialize: function (extractionCallback,metadataFileName,metadataFileNameDescription,metadataKeyVariable,metadataForeignFileName,metadataForeignKeyVariableName,metadataReferenceVariable,metdataOkBtn,inputFileNameRequired,inputNumberFirst,inputIllegalChar,outputOkId,okDataPathId,outputErrorId,outputNewExtractionId,newExtractionBtn,extractionTabId,outputExitId,exitBtn,structureTabId,fileNameLengthId,fileNameReservedWordId,fileDescrReqId) {
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
                settings.outputOkSpn = document.getElementById(outputOkId);
                settings.outputOkText = settings.outputOkSpn.innerHTML;
                settings.okDataPath = document.getElementById(okDataPathId);
                settings.outputErrorSpn = document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML; 
                settings.outputNewExtractionSpn = document.getElementById(outputNewExtractionId);
                settings.outputNewExtractionText = settings.outputNewExtractionSpn.innerHTML;
                settings.newExtractionBtn = document.getElementById(newExtractionBtn);
                settings.extractionTab = document.getElementById(extractionTabId);
                settings.outputExitSpn = document.getElementById(outputExitId); 
                settings.exitBtn = document.getElementById(exitBtn);
                settings.structureTab = document.getElementById(structureTabId);
                settings.fileNameLength = document.getElementById(fileNameLengthId);
                settings.fileNameReservedWord = document.getElementById(fileNameReservedWordId);
                settings.fileDescrReq = document.getElementById(fileDescrReqId);
                AddEvents();
            }
        }
    }(jQuery);