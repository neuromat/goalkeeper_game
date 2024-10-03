# Goalkeeper Game
The Goalkeeper's Game is a project developed by Neuromat to study the learning of deterministic and probabilistic processes.

Developed in Unity 5.6.3f1

## Documentation

Refer to this [repository](https://github.com/neuromat/goleiro-godot/wiki/Configuration#configuration-files)

## Use instructions

If you downloaded the game from the official NeuroMat project website, you should have the binary distribution (eg. *.exe* file) and a folder that contains the game data. We will refer to this folder as `_GameData_` folder.

Steps:
1. Copy the folder `CustomTrees` from `_GameData_/StreamingAssets` into `_GameData_`. It contains examples of [CustomTrees](https://github.com/neuromat/goleiro-godot/wiki/Configuration#configuration-files), which define the game options, including the "Kicker" (AI) behaviour.
1. Update the file `CustomTrees/index.info` with the list of *Adversary Teams* that will show in the game menu. The teams' names should match the names of folders within `CustomTrees`.
    >You can hide a team from the game menu by omitting it from the list in `index.info`.
1. Launch the game.

## Building from source

> This project was developed in Unity Editor 5.6.1f1, which is no longer supported. You can still download this version for Windows and MacOS from Unity's [archive](https://unity.com/releases/editor/archive). We **DO NOT** recommend using a more recent version, since those introduced breaking changes to the project structure.

1. Download the source or clone this repository into a working directory;
1. Open the Unity Editor. In the *Projects* tab, select the **OPEN** option and then the source code project folder;
1. In the editor *File > Build Settings*, mark all options in *Scenes In Build* section, chose a *Platform* and *Build*. Select the folder in which the artifacts will be delivered;
1. Follow the steps in [Use Instructions](#use-instructions)