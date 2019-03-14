/*
  implments all interaction events between main process and renderer process
  File dialogs & Popups
*/
const {ipcMain, dialog} = require('electron')

ipcMain.on('structure-open-file-dialog', (event) => {
  dialog.showOpenDialog({
    properties: ['openFile', 'openDirectory']
    }, (files) => {
    if (files) {
      event.sender.send('structure-selected-directory', files)
    }
  })
})

ipcMain.on('dataextraction-open-file-dialog', (event) => {
  dialog.showOpenDialog({
    properties: ['openFile'],
    filters: [
      {
        "name": "statistics file",
        "extensions": ["sas7bdat", "sav", "dta"]
      }
    ]  
  }, (files) => {
    if (files) {
      event.sender.send('dataextraction-selected-statistics-file', files)
    }
  })
})

ipcMain.on('indexfiles-open-archiveindex-file-dialog', (event) => {
  dialog.showOpenDialog({
    properties: ['openFile'],
    filters: [
      {
        "name": "archiveindex file",
        "extensions": ["xml"]
      }
    ]  
  }, (files) => {
    if (files) {
      event.sender.send('indexfiles-selected-archiveindex-file', files)
    }
  })
})

ipcMain.on('indexfiles-open-contextdocumentationindex-file-dialog', (event) => {
  dialog.showOpenDialog({
    properties: ['openFile'],
    filters: [
      {
        "name": "contextdocumentationindex file",
        "extensions": ["xml"]
      }
    ]  
  }, (files) => {
    if (files) {
      event.sender.send('indexfiles-selected-contextdocumentationindex-file', files)
    }
  })
})

ipcMain.on('validation-open-file-dialog', (event) => {
  dialog.showOpenDialog({
    properties: ['openFile', 'openDirectory']
  }, (files) => {
    if (files) {
      event.sender.send('validation-selected-directory', files)
    }
  })
})

ipcMain.on('open-information-dialog', (event,title,text) => {
  const options = {
    type: 'info',
    title: title,
    message: text,
    buttons: []
  }
  dialog.showMessageBox(options, (index) => {
    event.sender.send('information-dialog-selection', index)
  })
})

ipcMain.on('open-warning-dialog', (event,title,text) => {
  const options = {
    type: 'warning',
    title: title,
    message: text,
    buttons: []
  }
  dialog.showMessageBox(options, (index) => {
    event.sender.send('warning-dialog-selection', index)
  })
})

ipcMain.on('open-confirm-dialog', (event,title,text) => {
  const options = {
    type: 'warning',
    title: title,
    message: text,
    cancelId: 1,
    buttons: ['FORSÆT', 'FORTRYD']
  }
  dialog.showMessageBox(options, (index) => {
    event.sender.send('confirm-dialog-selection', index)
  })
})

ipcMain.on('open-error-dialog', (event,title,text) => {
  const options = {
    type: 'error',
    title: title,
    message: text,
    buttons: []
  }
  dialog.showMessageBox(options, (index) => {
    event.sender.send('error-dialog-selection', index)
  })
})