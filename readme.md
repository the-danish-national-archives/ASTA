File Converter
==============

## About this app
This Electron app is creted from scratch using the guides found at:
https://electronjs.org/

The menu item and navigation is created based on the guide found at:
https://www.christianengvall.se/electron-app-navigation/

This is created using _only_ open source and free license.

## Installation guide

* In order to make this project work, you need to have node.js installed. Install it via your terminal or from https://nodejs.org/en/
* Create a local path for the project and either clone the repository or download the zip (and unzip) file to that path.
* Navigate to the root folder of the project and run the command 'npm install' from your terminal (this will install all dependencies from the package.json file).
* Once the packages have been installed run the command 'npm start' from your terminal and the project will start up.

## Basic architecture 

This project is a minimalistic app created using Electron. It follows the conventions mentioned at https://electronjs.org/ for the basic setup.

* The main window creation and main process is handled from main.js.
* The container window for all the various elements is created in index.html
* All additional resources are located in the assets folder.
* * The sidebar (menu) logic is handled in the menu.js file.
* * All menu items have their own HTML file.

## Guide to packaging
In order to make a package of the program, use the scripts mentioned in the package.json file.
Under the section 'scripts' there is listed scripts for both packaging and creation of the installer files.

## Windows .exe issue
There can be an issue (the app will open twice) with creating a new .exe file for Windows, when one already exists. 
It is recommended to delete the contents of the release folder, even though the script is set to overwrite the contents.

## Branching strategy
In order to make the program development proceed as smoothly as possible, the branching strategy in the picture below have been implemented

![Branching](https://github.com/the-danish-national-archives/1007plus/blob/master/docs/Branching.PNG "Branching")

In the beginning of a sprint (or when the branch is needed), a branch is created from the master branch to the seperate programs. At the end of the sprint these branches are merged back into master. If needed branches can be made from the individual program branches. 
**Note** that due to limitations in the program used to make the diagram, it looks as if the branches are created from the one above. This is not true, all branches are created from the master branch, and merged back into that.