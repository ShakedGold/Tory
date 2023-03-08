# Tory
The all in one file converter, from videos to documents, this program can convert a lot of different file formats.


## Roadmap
- [x] Video Conversion Support
- [ ] Image Conversion Support
- [x] Audio Conversion Support
- [ ] Document Conversion Support
- [ ] Settings for each format
- [ ] General Settings
- [ ] More Tools for files (Such an Audio & Video seperator)
## Screenshots

![Video Conversion Section](/Screenshots/VideoConversion.png)

## Build & Run Locally
Make sure Windows SDK 10 (v.19041) and WinUI3 are installed

Clone this repository
```bash
git clone https://github.com/ShakedGold/Tory
```

Open the .sln file in Visual Studio
```bash
start ToryNew.sln
```

Open ```Package.appxmanifest``` in the "ToryNew (Package)", Click on the "Packaging" tab and choose a certificate or create a new one:
  1. Click on Create
  2. Set the publisher name to your name
  3. Click OK

Manage the Nuget Packages (Right click the solution and "Manage Nuget Packages for Solution") and add these Package Sources (Top right settings gear and click Package source and click on the +):
 - https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-Labs/nuget/v3/index.json
 - https://pkgs.dev.azure.com/dotnet/CommunityToolkit/_packaging/CommunityToolkit-MainLatest/nuget/v3/index.json

Press the Start Button


## APIs Used

| API | Version     | Description                |
| :-------- | :------- | :------------------------- |
| ![WindowsAppSDK](https://github.com/microsoft/windowsappsdk) | `1.2.221109.1` | the WinUI API used in Visual Studio |
| ![xFFmpeg.NET](https://github.com/cmxl/FFmpeg.NET) | `7.1.3` | an ffmpeg wrapper for the .NET framework |
| ![Windows Community ToolKit](https://github.com/CommunityToolkit/WindowsCommunityToolkit) | `7.1.2` | A collection of premade assets to use as controls |
| ![Windows Community ToolKit Labs](https://github.com/CommunityToolkit/Labs-Windows) | `0.0.11` | A collection of premade labs assets to use as controls |
