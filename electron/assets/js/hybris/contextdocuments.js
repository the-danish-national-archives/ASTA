/*
    Model is responsible for upload context documents
    initialize interface inputs: elements from <div id="hybris-panel-contextdocuments">
    output context documents at /ContextDocumentation
 */
window.Rigsarkiv = window.Rigsarkiv || {},
function (n) {
    Rigsarkiv.Hybris = Rigsarkiv.Hybris || {},
    function (n) {
        const { ipcRenderer } = require('electron');
        const {shell} = require('electron');
        const path = require('path');
        const os = require('os');
        const fs = require('fs');
        const domParser = require('xmldom');

        //private data memebers
        var settings = {
            printBtn: null,
            outputErrorSpn: null,
            outputErrorText: null,
            uploadsTbl: null,
            outputEmptyFileTitle: null,
            outputEmptyFileText: null,
            overviewTab: null,
            outputOkInformationTitle: null,
            outputOkInformationText: null,
            spinner: null,
            spinnerClass: null,
            selectDeliveryPackage: null,
            okConfirm: null,
            cancelConfirm: null,
            outputNextConfirmTitle: null,
            outputNextConfirmText: null,
            validateBtn: null,
            validateBtnText: null,
            documents: [],
            logs: [],
            hasSelected: false,
            documentsPath: null,
            filePath: null,
            filePostfix: "{0}_ASTA_contextdocuments.html",
            templateFileName: "contextdocuments.html",
            scriptPath: "./assets/scripts/{0}",
            resourceWinPath: "resources\\{0}",
            contextDocumentationFolder: "ContextDocumentation",
            docCollectionFolderName: "docCollection1"
        }

        //reset status & input fields
        var Reset = function () {
            settings.outputErrorSpn.hidden = true;
            $("#{0} tr:not(:first-child)".format(settings.uploadsTbl.id)).remove();
            settings.documents = [];
            settings.documentsPath = null;
            settings.hasSelected = false;
        }

        //get JSON upload document by id
        var GetDocument = function (id) {
            var result = null;
            settings.documents.forEach(upload => {
                if(upload.id === id) {
                    result = upload;
                }
            });
            return result;
        }

        // Update file control
        var UpdatePath = function(upload)
        {
            var path = (settings.documentsPath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.documentsPath,upload.id) : "{0}/{1}".format(settings.documentsPath,upload.id);
            if(fs.existsSync(path)) {
                var files = fs.readdirSync(path);
                if(files != null && files.length > 0) {
                    upload.path = (settings.documentsPath.indexOf("\\") > -1) ? "{0}\\{1}".format(path,files[0]) : "{0}/{1}".format(path,files[0]);
                    settings.hasSelected = true;
                }
            }
        }

        // Render Documents control
        var RenderDocuments = function(data) {
            if(data != null && data.toString() != null && data.toString() !== "") {
                EnsurePath();
                var doc = new domParser.DOMParser().parseFromString(data.toString());
                for(var i = 0; i < doc.documentElement.childNodes.length;i++) {
                    var node = doc.documentElement.childNodes[i];
                    if(node.nodeName === "document" && node.childNodes != null) {
                        var upload = {"id":"", "title":"", "path":""};
                        for(var j = 0; j < node.childNodes.length;j++) {
                            if(node.childNodes[j].nodeName === "documentID") {
                                upload.id = node.childNodes[j].firstChild.data;
                            }
                            if(node.childNodes[j].nodeName === "documentTitle") {
                                upload.title = node.childNodes[j].firstChild.data;
                            }
                        }
                        settings.documents.push(upload);                         
                    }
                }
                settings.documents.forEach(upload => {
                    if(Rigsarkiv.Hybris.Base.callback().mode === "Edit") { UpdatePath(upload); }
                    $(settings.uploadsTbl).append("<tr><td>{0}</td><td>{1}</td><td><input type=\"text\" id=\"hybris-contextdocuments-document-{0}\" class=\"path\" value=\"{2}\" readonly=\"true\" placeholder=\"Vælg sti med knappen\"/><button class=\"docBtn\" id=\"hybris-contextdocuments-selectFile-{0}\">Browse</button></td></tr>".format(upload.id,upload.title,upload.path));
                    document.getElementById("hybris-contextdocuments-selectFile-{0}".format(upload.id)).addEventListener('click', (event) => {
                        ipcRenderer.send('contextdocuments-open-file-dialog',upload.id);
                    })
                }); 
            }
            else {
                ipcRenderer.send('open-error-dialog',settings.outputEmptyFileTitle.innerHTML,settings.outputEmptyFileText.innerHTML);
            }
        }

        //Upload documents
        var EnsureDocuments = function () {
            settings.documents.forEach(upload => {
                if(upload.path !== "") {
                    var enableCopy = true;
                    var folders = upload.path.getFolders();
                    var fileName = folders[folders.length - 1];
                    var fileExt = fileName.substring(fileName.indexOf(".") + 1);
                    var path = (settings.documentsPath.indexOf("\\") > -1) ? "{0}\\{1}\\1.{2}".format(settings.documentsPath,upload.id,fileExt) : "{0}/{1}/1.{2}".format(settings.documentsPath,upload.id,fileExt);
                    if(Rigsarkiv.Hybris.Base.callback().mode === "Edit") {
                        if(path === upload.path) { enableCopy = false; }
                       if(enableCopy) {
                            var folderPath = (settings.documentsPath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.documentsPath,upload.id) : "{0}/{1}".format(settings.documentsPath,upload.id);                        
                            var files = fs.readdirSync(folderPath);
                            if(files != null && files.length > 0) {
                                console.logInfo(`delete file: ${files[0]} at ${folderPath}`,"Rigsarkiv.Hybris.ContextDocuments.EnsureDocuments");
                                var filePath = (settings.documentsPath.indexOf("\\") > -1) ? "{0}\\{1}".format(folderPath,files[0]) : "{0}/{1}".format(folderPath,files[0]);
                                fs.unlinkSync(filePath); 
                            }
                        }
                    } 
                    if(enableCopy) {
                        console.logInfo(`copy file: ${fileName} to ${path}`,"Rigsarkiv.Hybris.ContextDocuments.EnsureDocuments");
                        fs.copyFileSync(upload.path, path);
                    }                    
                }
            });
        }

        //ensure documents path
        var EnsurePath = function() {
            var destPath = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath;
            settings.documentsPath = (destPath.indexOf("\\") > -1) ? "{0}\\{1}\\{2}".format(destPath,settings.contextDocumentationFolder,settings.docCollectionFolderName) : "{0}/{1}/{2}".format(destPath,settings.contextDocumentationFolder,settings.docCollectionFolderName);
        }

        //Ensure documents folder Structure 
        var EnsureStructure = function () {
            EnsurePath();
            if(!fs.existsSync(settings.documentsPath)) {
                console.logInfo(`Create documents Path: ${settings.filePath}`,"Rigsarkiv.Hybris.ContextDocuments.EnsureStructure");
                fs.mkdirSync(settings.documentsPath, { recursive: true });
            }
            var path = null;
            settings.documents.forEach(upload => {
                path = (settings.documentsPath.indexOf("\\") > -1) ? "{0}\\{1}".format(settings.documentsPath,upload.id) : "{0}/{1}".format(settings.documentsPath,upload.id);
                if(!fs.existsSync(path)) {                        
                    console.logInfo(`create document path: ${path}`,"Rigsarkiv.Hybris.ContextDocuments.EnsureStructure");
                    fs.mkdirSync(path, { recursive: true });
                }
            });
        }

        //commit print data
        var EnsureData = function() {
            var data = fs.readFileSync(settings.filePath);        
            var folders = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath.getFolders();
            var folderName = folders[folders.length - 1];
            var updatedData = data.toString().format(folderName,settings.logs.join("\r\n"));
            fs.writeFileSync(settings.filePath, updatedData);                         
            settings.logs = [];                               
        }

        //copy HTML template file to parent folder of selected Delivery Package folder
        var CopyFile = function() {
            var filePath = settings.scriptPath.format(settings.templateFileName);        
            if(!fs.existsSync(filePath)) {
                var rootPath = null;
                if(os.platform() == "win32") {
                    rootPath = path.join('./');
                    filePath = path.join(rootPath,settings.resourceWinPath.format(settings.templateFileName));
                }
                if(os.platform() == "darwin") {
                    var folders =  __dirname.split("/");
                    rootPath = folders.slice(0,folders.length - 3).join("/");
                    filePath = "{0}/{1}".format(rootPath,settings.templateFileName);
                }
            }        
            console.logInfo(`copy ${settings.templateFileName} file to: ${settings.filePath}`,"Rigsarkiv.Hybris.ContextDocuments.CopyFile");
            fs.copyFileSync(filePath, settings.filePath);
            EnsureData();        
        }

        //enable/diable waiting spinner
        var UpdateSpinner = function(spinnerClass) {
            var disabled = (spinnerClass === "") ? false : true;
            settings.spinner.className = spinnerClass;
            settings.printBtn.disabled = disabled;
            settings.nextBtn.disabled = disabled;
            $("button[id^='hybris-contextdocuments-selectFile-']").each(function() {
                this.disabled = disabled;
            });                    
        }

        //add Event Listener to HTML elmenets
        var AddEvents = function () {
            settings.nextBtn.addEventListener('click', function (event) {
                if(settings.documents.length > 0) {                    
                    UpdateSpinner(settings.spinnerClass);
                    EnsureStructure();
                    UpdateSpinner("");                    
                }
                if(!settings.hasSelected) {
                    ipcRenderer.send('open-confirm-dialog','contextdocuments',settings.outputNextConfirmTitle.innerHTML,settings.outputNextConfirmText.innerHTML,settings.okConfirm.innerHTML,settings.cancelConfirm.innerHTML);
                }
                else {
                    if(settings.documents.length > 0) {                    
                        UpdateSpinner(settings.spinnerClass);
                        EnsureDocuments();
                        UpdateSpinner("");                    
                        //ipcRenderer.send('open-information-dialog',settings.outputOkInformationTitle.innerHTML,settings.outputOkInformationText.innerHTML);
                    }
                    settings.selectDeliveryPackage.innerHTML = "[{0}]".format(Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath);
                    var folders = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath.getFolders();
                    var folderName = folders[folders.length - 1];
                    settings.validateBtn.innerText = settings.validateBtnText.format(folderName);
                    settings.overviewTab.click();
                }
            });
            ipcRenderer.on('confirm-dialog-selection-contextdocuments', (event, index) => {
                if(index === 0) {
                    settings.selectDeliveryPackage.innerHTML = "[{0}]".format(Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath);
                    var folders = Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath.getFolders();
                    var folderName = folders[folders.length - 1]; 
                    settings.validateBtn.innerText = settings.validateBtnText.format(folderName);
                    settings.overviewTab.click();
                }            
            });
            settings.printBtn.addEventListener('click', function (event) {
                settings.filePath = settings.filePostfix.format(Rigsarkiv.Hybris.Structure.callback().deliveryPackagePath);
                settings.documents.forEach(upload => {
                    settings.logs.push("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>".format(upload.id,upload.title,upload.path));
                });
                CopyFile();
                shell.openItem(settings.filePath);
            });
            ipcRenderer.on('contextdocuments-selected-file', (event, path, id) => {
                console.logInfo(`selected document ${id} with path: ${path}`,"Rigsarkiv.Hybris.ContextDocuments.AddEvents");
                var upload = GetDocument(id);
                upload.path = path[0]; 
                document.getElementById("hybris-contextdocuments-document-{0}".format(upload.id)).value = upload.path; 
                settings.hasSelected = true;         
            })
        }
        
        //Model interfaces functions
        Rigsarkiv.Hybris.ContextDocuments = {
            initialize: function (outputErrorId,uploadsId,printId,outputEmptyFileId,nextId,overviewTabId,outputOkInformationPrefixId,spinnerId,selectDeliveryPackageId,outputOkConfirmId,outputCancelConfirmId,outputNextConfirmId,validateId) {
                settings.outputErrorSpn =  document.getElementById(outputErrorId);
                settings.outputErrorText = settings.outputErrorSpn.innerHTML;
                settings.uploadsTbl = document.getElementById(uploadsId);
                settings.printBtn = document.getElementById(printId);
                settings.outputEmptyFileTitle = document.getElementById(outputEmptyFileId + "-Title");
                settings.outputEmptyFileText = document.getElementById(outputEmptyFileId + "-Text");
                settings.nextBtn = document.getElementById(nextId);
                settings.overviewTab = document.getElementById(overviewTabId);
                settings.outputOkInformationTitle = document.getElementById(outputOkInformationPrefixId + "-Title");
                settings.outputOkInformationText = document.getElementById(outputOkInformationPrefixId + "-Text");
                settings.spinner = document.getElementById(spinnerId);
                settings.spinnerClass = settings.spinner.className;
                settings.spinner.className = "";
                settings.selectDeliveryPackage = document.getElementById(selectDeliveryPackageId);
                settings.okConfirm = document.getElementById(outputOkConfirmId);
                settings.cancelConfirm = document.getElementById(outputCancelConfirmId);
                settings.outputNextConfirmTitle = document.getElementById(outputNextConfirmId + "-Title");
                settings.outputNextConfirmText = document.getElementById(outputNextConfirmId + "-Text");
                settings.validateBtn = document.getElementById(validateId);
                settings.validateBtnText = settings.validateBtn.innerText;
                AddEvents();
            },
            callback: function () {
                return {
                    reset: function()  {
                        Reset();
                    },
                    load: function(data) {
                        try {
                            Reset();
                            RenderDocuments(data);
                        }
                        catch(err) {
                            err.Handle(settings.outputErrorSpn,settings.outputErrorText,"Rigsarkiv.Hybris.ContextDocuments.callback.load");
                        } 
                    }
                }
            }
        }
    }(jQuery);
}(jQuery);