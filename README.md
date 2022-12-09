# MiCellAnnGELo

MiCellAnnGELo (**Mi**croscopy and **Cell** **Ann**otation **G**raphical **E**xperience and **L**abelling to**o**l)
allows annotation of colour surfaces segmented from microscopy movies using either
a virtual reality (VR) headset and controllers, or computer screen with mouse and keyboard.

# Installation & Execution

## VR requirements

In order to make use of the VR functionality of the software, SteamVR must be installed.
This can be downloaded from the 
[SteamVR page](https://store.steampowered.com/app/250820/SteamVR/).

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

# Demonstration videos

Uncompressed demonstration videos can be downloaded [here](https://arxiv.org/abs/2209.11672).

<!-- TODO: add compressed videos -->

# Viewing source

## Viewing created scripts, shaders, etc

All created elements in the system are located in MiCellAnnGELo/Assets/.

## Viewing the project in Unity

To view the project in the Unity editor, install the [Unity Hub](https://store.unity.com/#plans-individual). In the projects tab, add the MiCellAnnGELo folder as a project and download the associated editor version (2019.4.15) through Unity Hub with build tools for Windows, MacOS, and Linux. After the editor is downloaded, the project can be opened by selecting it in Unity Hub.

The project may take multiple attempts to open due to a window layout error.

### Building the project

To rebuild the project from the Unity Editor, select Build Settings in the File menu. From here, select the correct OS and click Build. The project will build to the given file path.

# Publications

Please refer to [this preprint](https://arxiv.org/abs/2209.11672) for more information. Citation:
 > Platt, A., Lutton, E. J., Offord, E., & Bretschneider, T. (2022). MiCellAnnGELo: Annotate microscopy time series of complex cell surfaces with 3D Virtual Reality. arXiv preprint arXiv:2209.11672.
