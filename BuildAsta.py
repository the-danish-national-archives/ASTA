#!/usr/bin/python
"""BuildAsta

   Script to build all elements for Asta
   Depending on the underlying platform different configurations is used.

   The script builds the two dotnet submodules (Athena
   and Styx) if windows is detected
   Otherwise the corresponding package is made to a installer
   both for the normal and the extended version

Usage:
  BuildAsta.py [-fv] -u <ident> [--mode=<bm>]

Options:
  -h --help    Show this screen.
  --version    Show version.
  --mode=<bm>  Buildmode normal, extended or all [default: x].
  -u=<ident>   User ident [default: jenkins].
  -v           Verbose
  -f           Run audit fix [default: False]

"""
import json
import sys
import getopt
import os.path
import platform
import subprocess
import time
from docopt import docopt, DocoptExit
from pydoc import doc


""" Class Settings are created to hold the configuration
The default values are set to fit running on Jenkins windows node """


class Settings:
    def __init__(self,
                 min_node_version=12,
                 min_npm_version=6,
                 msbuild_path='C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\msbuild.exe',
                 nuget_path='C:\\ProgramData\\chocolatey\\bin\\NuGet.exe',
                 path_to_athena='./ASTA/dotNET/Athena.sln',
                 path_to_styx='./ASTA/dotNET/Styx.sln',
                 path_to_assets='./electron/assets/scripts/',
                 build_mode='a',
                 audit_fix=False):
        self.min_nodejs_version = min_node_version
        self.min_npm_version = min_npm_version
        self.msbuild_path = msbuild_path
        self.nuget_path = nuget_path
        self.path_to_athena = path_to_athena
        self.path_to_styx = path_to_styx
        self.path_to_assets = path_to_assets
        self.build_mode = build_mode
        self.audit_fix = audit_fix

    @property
    def msbuild_path(self):
        return self._msbuild_path

    @msbuild_path.setter
    def msbuild_path(self, value):
        self._msbuild_path = value

    @property
    def path_to_styx(self):
        return self._path_to_styx

    @path_to_styx.setter
    def path_to_styx(self, value):
        self._path_to_styx = value

    @property
    def path_to_athena(self):
        return self._path_to_athena

    @path_to_athena.setter
    def path_to_athena(self, value):
        self._path_to_athena = value

    @property
    def path_to_assets(self):
        return self._path_to_assets

    @path_to_assets.setter
    def path_to_assets(self, value):
        self._path_to_assets = value

    @property
    def min_nodejs_version(self):
        return self._min_nodejs_version

    @min_nodejs_version.setter
    def min_nodejs_version(self, value):
        if value < 12:  # Current version date 20200826
            raise ValueError("min_nodejs_version have to be 12 or higher")
        self._min_nodejs_version = value

    @property
    def min_npm_version(self):
        return self._min_npm_version

    @min_npm_version.setter
    def min_npm_version(self, value):
        if value < 6:  # Current version date 20200826
            raise ValueError("min_npm_version have to be 6 or higher")
        self._min_npm_version = value

    @property
    def nuget_path(self):
        return self._nuget_path

    @nuget_path.setter
    def nuget_path(self, value):
        self._nuget_path = value

    @property
    def build_mode(self):
        return self._build_mode

    @build_mode.setter
    def build_mode(self, value):
        legal_modes = {'n', 'x', 'a'}
        if value not in legal_modes:
            raise ValueError(
                "build_mode have to be one of theese values: 'n', 'x' or 'a'. Not '" + value + "'")
        self._build_mode = value

    @property
    def audit_fix(self):
        return self._audit_fix

    @audit_fix.setter
    def audit_fix(self, value):
        self._audit_fix = value


settings = Settings()

""" Function checking the prerequisites
such as msbuild, node and npm are present and in a minimum version"""


def verify_prerequisites(current_os):
    clear_to_go_ahead = True

    if current_os == "Windows":
        if not os.path.exists(settings.msbuild_path):
            print(settings.msbuild_path + '  is missing (or path is wrong).')
            clear_to_go_ahead = False

        if not os.path.isfile(settings.path_to_styx):
            print(os.getcwd() + settings.path_to_styx +
                  '  is missing (or path is wrong)')
            clear_to_go_ahead = False

        if not os.path.isfile(settings.path_to_athena):
            print(os.getcwd() + settings.path_to_athena +
                  '  is missing (or path is wrong)')
            clear_to_go_ahead = False

        if not os.path.isfile(settings.nuget_path):
            print("'{0}' is missing (or path is wrong)".format(settings.nuget_path))
            clear_to_go_ahead = False

    node_version = int(subprocess.run(
        'node --version', shell=True, capture_output=True, text=True).stdout[1:3])
    if not (node_version >= settings.min_nodejs_version):
        print("node ver {0} is missing or needs an update".format(str(node_version)))
        clear_to_go_ahead = False

    npm_version = int(subprocess.run('npm --version', shell=True,
                                     capture_output=True, text=True).stdout[0:1])
    if not (npm_version >= settings.min_npm_version):
        print("npm ver {0} is missing or needs an update".format(settings.min_npm_version))
        clear_to_go_ahead = False

    return clear_to_go_ahead


"""build_dotnet execution build commands til msbuild"""


def build_dotnet(cmds):
    for x in cmds:
        print(x)
        subprocess.run(x, shell=True)


"""Function to build the Release version of Athena (only on windows)
restore nuget packages and build to the release lib"""


def build_athena():
    print("Building Athena..")
    build_dotnet({'"{0}" restore {1}'.format(settings.nuget_path,settings.path_to_athena),
                  '"{0}" /t:Build /p:Configuration=Release {1}'.format(settings.msbuild_path,settings.path_to_athena)})
    print("last modified: %s" % time.ctime(
        os.path.getmtime(settings.path_to_assets + "Athena.dll")))


"""Function to build the Release version of Styx (only on windows)
restore nuget packages and build to the release lib"""


def build_styx():
    print("Building Styx..")
    build_dotnet(
        {'{0} restore {1}'.format(settings.nuget_path,settings.path_to_styx),
         '"{0}" /t:Build /p:Configuration=Release {1}'.format(settings.msbuild_path,settings.path_to_styx)})
    print("last modified: %s" % time.ctime(
        os.path.getmtime(settings.path_to_assets + "Styx.dll")))


"""Function to build the Release version of Asta
   On Mac and Windows there are build an installer
   On Windows there is an extended version (with Styx and Athena) included """


def build_asta(current_os, Settings: settings):
    print("Building Asta on {0}..".format(current_os))
    if settings.audit_fix:
        subprocess.run('npm audit fix', shell=True, cwd="electron")
    else:
        subprocess.run('npm audit', shell=True, cwd="electron")

    subprocess.run('npm install electron-packager -g',
                   shell=True, cwd="electron")
    subprocess.run('npm install npm-platform-dependencies',
                   shell=True, cwd="electron")
    subprocess.run('npm install', shell=True, cwd="electron")
    if current_os == "Windows":
        subprocess.run('npm run package-win', shell=True, cwd="electron")
        if settings.build_mode == 'n' or settings.build_mode == 'a':
            subprocess.run('npm run create-installer-win',
                           shell=True, cwd="electron")
        if settings.build_mode != 'x' or settings.build_mode == 'a':
            subprocess.run('npm run package-win-extended',
                           shell=True, cwd="electron")
            subprocess.run('npm run create-installer-win-extended',
                           shell=True, cwd="electron")

    if current_os == "Linux":
        if settings.build_mode == 'n' or settings.build_mode == 'a':
            subprocess.run('npm run package-linux', shell=True, cwd="electron")
        if settings.build_mode != 'x' or settings.build_mode == 'a':
            subprocess.run('npm run package-linux-extended',
                           shell=True, cwd="electron")

    if current_os == "Darwin":
        subprocess.run('npm install electron-packager -g',
                       shell=True, cwd="electron")
        if settings.build_mode == 'n' or settings.build_mode == 'a':
            subprocess.run('npm run package-mac', shell=True, cwd="electron")
            subprocess.run('npm run create-installer-mac',
                           shell=True, cwd="electron")
        if settings.build_mode == 'x' or settings.build_mode == 'a':
            subprocess.run('npm run package-mac-extended',
                           shell=True, cwd="electron")
            subprocess.run('npm run create-installer-mac-extended',
                           shell=True, cwd="electron")


""" Detect the current platform
    The script only handles three platforms and aborts if either it cant
     detect the platform or if its not one of the tree (Linux', 'Darwin', 'Windows')"""


def detect_platform() -> str:
    list_of_platforms = {'Linux', 'Darwin', 'Windows'}
    if not platform.system() in list_of_platforms:
        print("\nUnknown platform - no reason to continue. Halt!")
        quit()
    return str(platform.system())


""" This is the place for setting the general values for settings """


def config_all(arguments):
    settings.build_mode = arguments.get("--mode")
    settings.audit_fix = arguments.get("-f")


""" This is the place for setting the values for user 'kna' """


def config_kna():
    settings.nuget_path = 'D:\\Repos\\Asta\\electron\\node_modules\\electron-winstaller\\vendor\\nuget.exe'
    settings.msbuild_path = 'D:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\MSBuild\\Current\\Bin\\MSBuild.exe'


""" This is the place for setting the values for user 'tkn' """


def config_tkn():
    settings.nuget_path = ''
    settings.msbuild_path = ''


""" This is the place for setting the values for user 'rhr' """


def config_rhr():
    settings.nuget_path = ''
    settings.msbuild_path = ''

    """ Function for configuring the settings object
    If there is a parameter of type user verify its a known user
    and set it up accordingly by calling the matching config-function.
    If not set the user to the default Jenkins-user """


def setup_config(user: str, arguments):
    legal_users = {'kna', 'tkn', 'rhr', 'Jenkins'}

    if user not in legal_users:
        user = 'jenkins'
        print('Legal user not found - moving forward with "Jenkins" as user')
    print('user setting is "{0}"\n'.format(user))
    config_all(arguments)
    if user == 'kna':
        config_kna()
    if user == 'rhr':
        config_rhr()
    if user == 'tkn':
        config_tkn()


def main(arguments):
    user = arguments.get('-u')
 #   auditFix =  arguments.get('-f')
   # buildMode=arguments.get('--mode')
    if user == None:
        user = "Jenkins"
    verbose = False
    if arguments.get('-v'):
        verbose = True
    if verbose:
        print("Bruger: " + user)
    tic = time.perf_counter()  # Start timer

    setup_config(user, arguments)
    current_os = detect_platform()
    if verbose:
        jsonStr = json.dumps(settings.__dict__, indent=2)
        print("Configuration:\n" + jsonStr)
        print("Starting build artefacts...")
        print("\nPlatform identified as '{0}'\n".format(current_os))
    if verify_prerequisites(current_os):
        if current_os == 'Windows':
            build_styx()
            build_athena()

        build_asta(current_os, settings)

    toc = time.perf_counter()  # stop timer and print
    print(f"\nThe script was running in {toc - tic:0.4f} seconds")


if __name__ == '__main__':
    try:
        arguments = docopt(__doc__, argv=None, help=True,
                           version='Asta build script ver. 1.0')
        main(arguments)
    except DocoptExit:
        print(__doc__)
# To make a html documenting this run 'pycco buildasta.py' - required pycco installation fx. using pip
