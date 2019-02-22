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