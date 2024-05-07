# VideoPlayerExtentions
A ChilloutVR mod which adds features to the built-in Video Players

## Install

*this assumes you have MelonLoader installed already*

1. Download the [latest release](https://github.com/200Tigersbloxed/VideoPlayerExtentions/releases/latest)
2. Move to the `path/to/ChilloutVR/Mods` folder

> [!NOTE]  
> [BTKUILib](https://github.com/BTK-Development/BTKUILib) is recommended as it enables editing config values in-game. This is not required and all config values can be manually set from the `path/to/ChilloutVR/UserData/MelonPreferences.cfg` file.

## Features

### ForceDirect

Autmoatically sets the audio mode for all Video Players to `Direct` (2D audio source) when they are enabled.

### DyanmicLibVLC

Switches Video Player backends to LibVLC if AVPro cannot handle the file. This is useful for RTMP/RTSP and SRT streams.

> [!IMPORTANT]  
> You can control which protocols/files use LibVLC with the `VLCProtocols` and `VLCFiles` preferences. These cannot be set in-game.
