window.Rigsarkiv = window.Rigsarkiv || {},
    function (n) {
        const { ipcRenderer } = require('electron');
        const startNumberPattern = (/^([a-zA-ZæøåÆØÅ])([a-zA-ZæøåÆØÅ0-9]*)/)
        const quotesPattern = /^"([a-zA-ZæøåÆØÅ0-9]*)"$/;

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
            illegalChar: null
        }

        var Reset = function () {
            settings.fileNameReq.hidden = true;
            settings.numberFirst.hidden = true;
        }

        function ValidateFields() {
            var validationOk = true;
            if (startNumberPattern.test(settings.fileName.value) || quotesPattern.test(settings.fileName.value)){
                console.log("valid");
            } else {
                console.log('error');
            }


            // if (settings.fileName.value === "") {
            //     settings.fileNameReq.hidden = false;
            // }
            // if (startNumberPattern.test(settings.fileName.value)) {
            //     settings.numberFirst.hidden =  false;
            // }
            // else if (!quotesPattern.test(settings.fileName.value)) {
            //     settings.illegalChar.hidden = false;
            // }

        }

        var AddEvents = function () {
            settings.okBtn.addEventListener('click', function (event) {
                Reset();
                ValidateFields();
            })
        }

        Rigsarkiv.MetaData = {
            initialize: function (metadataFileName, metadataFileNameDescription, metadataKeyVariable, metadataForeignFileName, metadataForeignKeyVariableName, metadataReferenceVariable, metdataOkBtn,
                inputFileNameRequired, inputNumberFirst, inputIllegalChar) {
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