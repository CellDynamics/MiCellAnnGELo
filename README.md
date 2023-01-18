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

Demonstration videos shown below can be downloaded at full resolution [here](https://academic.oup.com/bioinformatics/advance-article-abstract/doi/10.1093/bioinformatics/btad013/6984716?utm_source=advanceaccess&utm_campaign=bioinformatics&utm_medium=email).

## Placing markers

https://user-images.githubusercontent.com/44062886/207293288-79ceb40e-b363-4e65-8a25-b7f176a5362f.mp4

## Painting surfaces

https://user-images.githubusercontent.com/44062886/207293495-af22e763-18b2-43e5-ba2f-223aa83c4d88.mp4

## Moving through frames and surface manipulation

https://user-images.githubusercontent.com/44062886/207295114-c06b9de0-9a49-4b5a-868b-df56103bc760.mp4

## Colour and opacity controls

https://user-images.githubusercontent.com/44062886/207295189-53f6503a-4f2d-4e39-a5d7-f28339f6c040.mp4

# Viewing source

## Viewing created scripts, shaders, etc

All created elements in the system are located in MiCellAnnGELo/Assets/.

## Viewing the project in Unity

To view the project in the Unity editor, install the [Unity Hub](https://store.unity.com/#plans-individual). In the projects tab, add the MiCellAnnGELo folder as a project and download the associated editor version (2019.4.15) through Unity Hub with build tools for Windows, MacOS, and Linux. After the editor is downloaded, the project can be opened by selecting it in Unity Hub.

The project may take multiple attempts to open due to a window layout error.

### Building the project

To rebuild the project from the Unity Editor, select Build Settings in the File menu. From here, select the correct OS and click Build. The project will build to the given file path.

# Publications

Please refer to [this publication](https://academic.oup.com/bioinformatics/advance-article-abstract/doi/10.1093/bioinformatics/btad013/6984716?utm_source=advanceaccess&utm_campaign=bioinformatics&utm_medium=email) for more information. Citation:
 > Platt, A., Lutton, E. J., Offord, E., & Bretschneider, T. (2023). MiCellAnnGELo: Annotate microscopy time series of complex cell surfaces with 3D Virtual Reality. <I>Bioinformatics</I>, btad013, https://doi.org/10.1093/bioinformatics/btad013
