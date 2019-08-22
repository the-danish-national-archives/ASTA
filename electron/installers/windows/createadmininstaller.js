const createWindowsInstaller = require('electron-winstaller').createWindowsInstaller
const path = require('path')
const fs = require('fs');

var scriptsPath = path.join('./assets/scripts');
var releasePath = path.join('./release-builds/windows-installer-admin/athena');
var files = ["log4net.dll","Asta.dll","Athena.dll","AthenaConsole.exe","AthenaConsole.exe.config","Styx.dll","StyxConsole.exe","StyxConsole.exe.config"];
files.forEach(file => {
  fs.copyFile(path.join(scriptsPath,file), path.join(releasePath,file), (err) => {
    if (err) throw err;
    console.log(file + ' was copied');
  })
});

getInstallerConfig()
  .then(createWindowsInstaller)
  .catch((error) => {
    console.error(error.message || error)
    process.exit(1)
  })

function getInstallerConfig () {
  console.log('creating windows admin installer')
  const rootPath = path.join('./')
  const outPath = path.join(rootPath, 'release-builds')

  return Promise.resolve({
    appDirectory: path.join(outPath, 'file-converter-admin-app-win32-ia32/'),
    authors: 'ProActive',
    noMsi: true,
    outputDirectory: path.join(outPath, 'windows-installer-admin'),
    exe: 'file-converter-admin-app.exe',
    setupExe: 'FileConverterAdmin.exe',
    setupIcon: path.join(rootPath, 'assets', 'icons', 'win', 'SA_Krone_App.ico')
  })
}