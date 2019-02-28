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
            isValidMetadata: true,
            metadataFileName: "metadata.txt",
            metadataFilePath: "./assets/scripts/{0}"
        }

        var HandleError = function(err) {
            console.log(`Error: ${err}`);
            //settings.outputStatisticsErrorSpn.hidden = false;
            //settings.outputStatisticsErrorSpn.innerHTML = settings.outputStatisticsErrorText.format(err.message);
        }

        var Reset = function () {
            settings.fileNameReq.hidden = true;
            settings.numberFirst.hidden = true;
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

        var EnsureFile = function() {
            var dataFolderPath = settings.extractionCallback().dataFolderPath;
            console.log(`copy metadat file to: ${dataFolderPath}`);
            fs.copyFile(settings.metadataFilePath.format(settings.metadataFileName), dataFolderPath + "/" + settings.metadataFileName, (err) => {
                if (err) {
                    HandleError(err);
                }
                else {
                    console.log(settings.metadataFileName + ' was copied to '+ settings.dataFolderPath);
                }
            });
        }

        var AddEvents = function () {
            settings.okBtn.addEventListener('click', function (event) {
                Reset();
                ValidateFields();
                if(settings.isValidMetadata) { EnsureFile(); }
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