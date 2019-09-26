/*
The script that runs in the main process can display a GUI by creating web pages. An Electron app always has one main process, but never more.
https://electronjs.org/docs/tutorial/application-architecture
*/
// Modules to control application life and create native browser window
const { ipcMain, app, BrowserWindow, dialog } = require('electron')
const setupEvents = require('./installers/setupEvents')
// Keep a global reference of the window object, if you don't, the window will
// be closed automatically when the JavaScript object is garbage collected.
let mainWindow
var preventClose = true;

if (setupEvents.handleSquirrelEvent()) {
  // squirrel event handled and app will exit in 1000ms, so don't do anything else
  return;
}

ipcMain.on('app-close', (event,title,text,okTest,cancelText) => {
  const options = {
    type: 'warning',
    title: title,
    message: text,
    cancelId: 1,
    buttons: [okTest, cancelText]
  }
  dialog.showMessageBox(options, (index) => {
    if(index === 0) 
    { 
      preventClose = false; 
      app.quit();
    }
  })  
})

function createWindow() {
  // Create the browser window.
  mainWindow = new BrowserWindow({
    width: 1300,
    height: 800,
    webPreferences: {
      nodeIntegration: true
    }
    //,icon: path.join(__dirname, 'assets/icons/png/<img>') <-- set this
  })

  // and load the index.html of the app.
  mainWindow.loadFile('index.html')

  // Open the DevTools.
  // mainWindow.webContents.openDevTools()

  // Emitted when the window is closed.
  mainWindow.on('closed', function () {
   // Dereference the window object, usually you would store windows
    // in an array if your app supports multi windows, this is the time
    // when you should delete the corresponding element.
    mainWindow = null
  })

  mainWindow.on('close', function (e) {
    if(preventClose) { 
      e.preventDefault();
      mainWindow.webContents.send('app-close');
    }
  })
}

// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.on('ready', createWindow)

// Quit when all windows are closed.
app.on('window-all-closed', function () {  
  // On macOS it is common for applications and their menu bar
  // to stay active until the user quits explicitly with Cmd + Q
  if (process.platform !== 'darwin') {
    app.quit()
  }
})

app.on('activate', function () {
  // On macOS it's common to re-create a window in the app when the
  // dock icon is clicked and there are no other windows open.
  if (mainWindow === null) {
    createWindow()
  }
})

// In this file you can include the rest of your app's specific main process
// code. You can also put them in separate files and require them here.
function makeSingleInstance() {
  if (process.mas) return

  app.requestSingleInstanceLock()

  app.on('second-instance', () => {
    if (mainWindow) {
      if (mainWindow.isMinimized()) mainWindow.restore()
      mainWindow.focus()
    }
  })
}

// Require each JS file in the main-process dir
require('./assets/js/main-process')