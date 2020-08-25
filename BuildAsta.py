import os.path
import platform
import subprocess
import time

# Constants
MIN_NODE_VERSION = 12
MIN_NPM_VERSION = 6
"""Edit the path to your location of MSBUILD"""
MSBUILD_PATH = 'C:\\Program Files (x86)\\Microsoft Visual Studio\\2019\\Enterprise\\MSBuild\\Current\\Bin\\msbuild.exe'
PATH_TO_ATHENA: str = './dotNET/Athena.sln'
PATH_TO_STYX = './dotNET/Styx.sln'
PATH_TO_ASSETS = './electron/assets/scripts/'

def verify_prerequisites(current_os):
    # Function checking the prerequisites
    # such as msbuild, node and npm are present and in a minimum version
    clear_to_go_ahead = True

    if current_os == "Windows":
        if not os.path.exists(MSBUILD_PATH):
            print(MSBUILD_PATH + ' mangler.')
            clear_to_go_ahead = False

        if not os.path.isfile(PATH_TO_STYX):
            print(os.getcwd() + PATH_TO_STYX + ' mangler')
            clear_to_go_ahead = False

        if not os.path.isfile(PATH_TO_ATHENA):
            print(os.getcwd() + PATH_TO_ATHENA + ' mangler')
            clear_to_go_ahead = False

    node_version = int(subprocess.run('node --version', shell=True, capture_output=True, text=True).stdout[1:3])
    if not (node_version >= MIN_NODE_VERSION):
        print(f'node {str(node_version)} mangler eller skal opdateres')
        clear_to_go_ahead = False

    npm_version = int(subprocess.run('npm --version', shell=True, capture_output=True, text=True).stdout[0:1])
    if not (node_version >= MIN_NODE_VERSION):
        print(f'npm {npm_version} mangler eller skal opdateres')
        clear_to_go_ahead = False

    return clear_to_go_ahead


def build_athena():
    # Function to build the Release version of Athena (only on windows)
    print("Building Athena..")
    cmd = f'"{MSBUILD_PATH}" /t:Build /p:Configuration=Release {PATH_TO_ATHENA}'
    subprocess.run(cmd, shell=True)
    print("last modified: %s" % time.ctime(os.path.getmtime(PATH_TO_ASSETS + "Athena.dll")))


def build_styx():
    # Function to build the Release version of Styx (only on windows)
    print("Building Styx..")
    cmd = f'"{MSBUILD_PATH}" /t:Build /p:Configuration=Release {PATH_TO_STYX}'
    print(cmd)
    subprocess.run(cmd, shell=True)
    print("last modified: %s" % time.ctime(os.path.getmtime(PATH_TO_ASSETS + "Styx.dll")))


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


def main():
    # Script to build all elements for Asta
    # Depending on the underlying platform different configurations is used
    tic = time.perf_counter()  # Start timer
    print("Starting build artefacts...")
    current_os = detect_platform()

    print(f"\nPlatform identified as '{current_os}'\n")

    if verify_prerequisites(current_os):
        if os == 'Windows':
            build_styx()
            build_athena()

        build_asta(current_os)

    toc = time.perf_counter()
    print(f"\nThe builds was DONE in {toc - tic:0.4f} seconds")  # stop timer and print


main()
