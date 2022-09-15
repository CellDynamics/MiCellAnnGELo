# MiCellAnnGELo

**Mi**croscopy and **Cell** **Ann**otation **G**raphical **E**xperience and **L**abelling to**o**l

# Installation & Execution

In order to make use of the VR functionality of the software, SteamVR must be installed. This can be downloaded on the [SteamVR page](https://store.steampowered.com/app/250820/SteamVR/) which requires Steam to be installed, downloadable from the [Steam download page](https://store.steampowered.com/about/).

After SteamVR has been installed and set up according to the instructions, the installing and execution of the system varies depending on OS. The distributions for each platform are located in the Build folder.

## Windows

Run the `MiCellAnnGELo.exe` file in the Windows build folder.

## MacOS

Run the following commands from terminal in the MacOS build folder:
```
chmod a+x MiCellAnnGELo.app/Contents/MacOS/*
xattr -r -d com.apple.quarantine MiCellAnnGELo.app/
```

After these commands have been run, the application can be run by running `MiCellAnnGELo.app`.

## Linux

Run the following Run the following command from terminal in the Linux build folder:
```
chmod +x MiCellAnnGELo.x86_64
```

After this command has been run, the application can be run using `./MiCellAnnGELo.x86_64`.

# Loading sample data

Sample surface files are located in SampleData/HM3477_XX0001.zip.
To load the surface files into MiCellAnnGELo, first unzip the data folder, then start MiCellAnnGELo.
If using desktop mode, first press <kbd>Tab</kbd> to enter desktop mode.
Use the "Load Time Series" button on the wall-mounted interface, then navigate to the unzipped folder and click "load".
Controls are displayed for both keyboard and VR controllers on the wall.

Surface data from other sources can be imported as a folder of .ply files with frame numbers in the file names.

# Viewing source

## Viewing created scripts, shaders, etc

All created elements in the system are located in MiCellAnnGELo/Assets/.

## Viewing the project in Unity

To view the project in the Unity editor, install the [Unity Hub](https://store.unity.com/#plans-individual). In the projects tab, add the MiCellAnnGELo folder as a project and download the associated editor version (2019.4.15) through Unity Hub with build tools for Windows, MacOS, and Linux. After the editor is downloaded, the project can be opened by selecting it in Unity Hub.

The project may take multiple attempts to open due to a window layout error.

### Building the project

To rebuild the project from the Unity Editor, select Build Settings in the File menu. From here, select the correct OS and click Build. The project will build to the given file path.
