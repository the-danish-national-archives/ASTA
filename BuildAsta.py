#!/usr/bin/python

import sys, getopt
import os.path
import platform
import subprocess
import time


class Settings:
    def __init__(self,
                 min_node_version=12,
                 min_npm_version=6,
                 msbuild_path='C:\\Program Files (x86)\\MSBuild\\14.0\\Bin\\msbuild.exe',
                 nuget_path='nuget.exe',
                 path_to_athena='./dotNET/Athena.sln',
                 path_to_styx='./dotNET/Styx.sln',
                 path_to_assets='./electron/assets/scripts/'):
        """default values are set to fit Jenkins"""
        self.min_nodejs_version = min_node_version
        self.min_npm_version = min_npm_version
        self.msbuild_path = msbuild_path
        self.nuget_path = nuget_path
        self.path_to_athena = path_to_athena
        self.path_to_styx = path_to_styx
        self.path_to_assets = path_to_assets

    @property
    def msbuild_path(self):
        return self._msbuild_path

    @msbuild_path.setter
    def msbuild_path(self, value):
        # if not os.path.exists(value):
        #     raise ValueError("msbuild_path have to exists")
        self._msbuild_path = value

    @property
    def path_to_styx(self):
        return self._path_to_styx

    @path_to_styx.setter
    def path_to_styx(self, value):
        # if not os.path.exists(value):
        #     raise ValueError(" Path to styx solution have to exists")
        self._path_to_styx = value

    @property
    def path_to_athena(self):
        return self._path_to_athena

    @property
    def path_to_assets(self):
        return self._path_to_assets

    @path_to_assets.setter
    def path_to_assets(self, value):
        if not os.path.exists(value):
            raise ValueError(" Path to assets have to exists")
        self._path_to_assets = value

    @path_to_athena.setter
    def path_to_athena(self, value):
        if not os.path.exists(value):
            raise ValueError(" Path to Athena solution have to exists")
        self._path_to_athena = value

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
        # if not os.path.exists(value):
        #     raise ValueError("nuget_path have to exists")
        self._nuget_path = value


settings = Settings()


def verify_prerequisites(current_os):
    # Function checking the prerequisites
    # such as msbuild, node and npm are present and in a minimum version
    clear_to_go_ahead = True

    if current_os == "Windows":
        if not os.path.exists(settings.msbuild_path):
            print(settings.msbuild_path + ' mangler.')
            clear_to_go_ahead = False

        if not os.path.isfile(settings.path_to_styx):
            print(os.getcwd() + settings.path_to_styx + ' mangler')
            clear_to_go_ahead = False

        if not os.path.isfile(settings.path_to_athena):
            print(os.getcwd() + settings.path_to_athena + ' mangler')
            clear_to_go_ahead = False

        if not os.path.isfile(settings.nuget_path):
            print(f'{settings.nuget_path} mangler')
            clear_to_go_ahead = False

    node_version = int(subprocess.run('node --version', shell=True, capture_output=True, text=True).stdout[1:3])
    if not (node_version >= settings.min_nodejs_version):
        print(f'node {str(node_version)} mangler eller skal opdateres')
        clear_to_go_ahead = False

    npm_version = int(subprocess.run('npm --version', shell=True, capture_output=True, text=True).stdout[0:1])
    if not (npm_version >= settings.min_npm_version):
        print(f'npm {settings.min_npm_version} mangler eller skal opdateres')
        clear_to_go_ahead = False

    return clear_to_go_ahead


def build_dotnet(cmds):
    for x in cmds:
        print(x)
        subprocess.run(x, shell=True)


def build_athena():
    # Function to build the Release version of Athena (only on windows)
    print("Building Athena..")
    build_dotnet({f'"{settings.nuget_path}" restore {settings.path_to_athena}',
                  f'"{settings.msbuild_path}" /t:Build /p:Configuration=Release {settings.path_to_athena}'})
    print("last modified: %s" % time.ctime(os.path.getmtime(settings.path_to_assets + "Athena.dll")))


def build_styx():
    # Function to build the Release version of Styx (only on windows)
    print("Building Styx..")
    build_dotnet(
        {f'{settings.nuget_path} restore {settings.path_to_styx}',
         f'"{settings.msbuild_path}" /t:Build /p:Configuration=Release {settings.path_to_styx}'})
    print("last modified: %s" % time.ctime(os.path.getmtime(settings.path_to_assets + "Styx.dll")))


def build_asta(current_os):
    # Function to build the Release version of Asta
    # On Mac and Windows there are build an installer
    # On Windows there is an extended version (with Styx and Athena) included
    print(f"Building Asta on {current_os}..")
    subprocess.run('npm install electron-packager -g', shell=True, cwd="electron")
    subprocess.run('npm install npm-platform-dependencies', shell=True, cwd="electron")
    subprocess.run('npm install', shell=True, cwd="electron")
    if current_os == "Windows":
        subprocess.run('npm run package-win', shell=True, cwd="electron")
        subprocess.run('npm run create-installer-win', shell=True, cwd="electron")
        subprocess.run('npm run package-win-extended', shell=True, cwd="electron")
        subprocess.run('npm run create-installer-win-extended', shell=True, cwd="electron")

    if current_os == "Linux":
        subprocess.run('npm run package-linux', shell=True, cwd="electron")
        subprocess.run('npm run package-linux-extended', shell=True, cwd="electron")

    if current_os == "Darwin":
        subprocess.run('npm install electron-packager -g', shell=True, cwd="electron")
        subprocess.run('npm run package-mac', shell=True, cwd="electron")
        subprocess.run('npm run create-installer-mac', shell=True, cwd="electron")
        subprocess.run('npm run package-mac-extended', shell=True, cwd="electron")
        subprocess.run('npm run create-installer-mac-extended', shell=True, cwd="electron")


def detect_platform() -> str:
    # Detect the current platform
    # The script only handles three platforms and aborts if either it cant
    # detect the platform or if its not one of the tree (Linux', 'Darwin', 'Windows')
    list_of_platforms = {'Linux', 'Darwin', 'Windows'}
    if not platform.system() in list_of_platforms:
        """unknown platform - no reason to continue"""
        print("\nUnknown platform - Halt!")
        quit()
    return str(platform.system())


def config_kna():
    settings.msbuild_path = 'D:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\MSBuild\\Current\\Bin\\MSBuild.exe'
    settings.nuget_path = 'C:\\FastRepos\\Asta\\electron\\node_modules\\electron-winstaller\\vendor\\nuget.exe'


def config_tkn():
    settings.nuget_path = ''
    settings.msbuild_path = ''


def config_rhr():
    settings.nuget_path = ''
    settings.msbuild_path = ''


def setup_config(argv):
    # Function for configuring the settings object
    # if there is a parameter of type user verify its a known user
    # and set it up accordingly by calling the matching config-function
    legal_users = {'kna', 'tkn', 'rhr', 'Jenkins'}
    user = ''
    try:
        opts, args = getopt.getopt(argv, "h:u:", ["user="])
    except getopt.GetoptError:
        print('BuildAsta.py -u <user>')
        sys.exit(2)
    for opt, arg in opts:
        if opt == '-h':
            print('BuildAsta.py -u <user>')
            print('Default user is Jenkins')
            print('Legal users are:')
            for p in legal_users:
                print(f'\t{p}')
            sys.exit()
        elif opt in ("-u", "--user"):
            user = arg.lower()

    if user not in legal_users:
        user = 'jenkins'
        print('Legal user not found - moving forward with "Jenkins" as user')
    print(f'user setting is "{user}"\n')
    if user == 'kna':
        config_kna()
    if user == 'rhr':
        config_rhr()
    if user == 'tkn':
        config_tkn()


def main(argv):
    # Script to build all elements for Asta
    # Depending on the underlying platform different configurations is used
    tic = time.perf_counter()  # Start timer

    setup_config(argv)
    print("Starting build artefacts...")
    current_os = detect_platform()

    print(f"\nPlatform identified as '{current_os}'\n")
    if verify_prerequisites(current_os):
        if current_os == 'Windows':
            build_styx()
            build_athena()

        build_asta(current_os)

    toc = time.perf_counter()
    print(f"\nThe builds was DONE in {toc - tic:0.4f} seconds")  # stop timer and print


main(sys.argv[1:])
