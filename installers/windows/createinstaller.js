const createWindowsInstaller = require('electron-winstaller').createWindowsInstaller
const path = require('path')

getInstallerConfig()
  .then(createWindowsInstaller)
  .catch((error) => {
    console.error(error.message || error)
    process.exit(1)
  })

function getInstallerConfig () {
  console.log('creating windows installer')
  const rootPath = path.join('./')
  const outPath = path.join(rootPath, 'release-builds')

  return Promise.resolve({
    appDirectory: path.join(outPath, 'file-converter-app-win32-ia32/'),
    authors: 'ProActive',
    noMsi: true,
    outputDirectory: path.join(outPath, 'windows-installer'),
    exe: 'file-converter-app.exe',
    setupExe: 'FileConverter.exe',
    setupIcon: path.join(rootPath, 'assets', 'icons', 'win', 'SA_Krone_App.ico')
  })
}