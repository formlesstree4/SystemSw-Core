# SystemSW-Sharp
A mostly complete open source implementation of the Extron System 8/10 API in the C# programming language


# Features
As I am not concerning myself with ALL the features of the Extron System 8/10 series, here's a rough idea of the feature set that is currently implemented inside SystemSW-Sharp:
1. Identification
1. Channel Switching
1. Independent Audio / Video Switching (for breakouts)
1. RGB Toggle
1. Audio Toggle
1. Projector Power Toggle
1. Projector Display Toggle

# Application Features
Currently, the application exposes only switching audio and video at the same time.
I do have future plans to support independent audio and video switching in the UI.

## Identification
As part of the boot process, an 'I' is submitted to the Extron in order to identify what the system actually is. This response string usually looks something like this:

```
V1 A1 T1 P0 S0 Z0 R0 QSC1.11 QPC1.11 M4
```

This response string is how SystemSW-Sharp can set itself up to prevent some silly mistakes from happening when writing an application against the API

## Configuring the UI
The current User Interface is a single page application developed in Angular that connects to a WebAPI instance.
Both the Angular application and the WebAPI instance **must** be on the same server that will be talking to the System 8/10. There is no current support for hosting the UI and API on separate servers.
In addition, both the UI and API must be accessible from outside your server.

### Port
The application supports changing the name of the serial port to listen to via `appsettings.json` in the WebAPI project. Changing this will require a reboot of the WebAPI project.

### Timeouts
The Read and Write timeout values can be adjusted. Currently, only Read timeouts are applied but write timeouts are to come in the future.

### 'Type'
There's a rather ominous configuration flag named 'Type'. Unless you are a developer doing some testing, don't adjust this value.
Anything other than 'Serial' will cause a dummied out Extron class to be created.

### AutoOpen
Unless there's a particular need, don't adjust the 'AutoOpen' value either.

### Mappings
The application supports named mappings via `appsettings.json` in the WebAPI project. Updating this project and reloading the page will show the changes in the mappings.
This collection of mappings does not have a hard coded limit and you may add more than 10 entries. However, the Extron will be the one to dictate how many entries show up on the UI.

Currently, the mappings are configured as such:
```json
  "ChannelMappings": {
    "1": "Super Nintendo",
    "2": "Wii",
    "3": "Nintendo 64",
    "4": "PS2 (JP)",
    "5": "Famicom",
    "7": "Sega Memecast",
    "8": "Playstation 2 (EN)"
  }
```

This will eventually be mapped internally to a Key-Value-Pair of <string, string>. The key is the switcher port on the Extron. The value is the name you want to show on the UI. Any unmapped values will render as a question mark on the UI.

# Building
In order to build the application, NodeJS will need to be downloaded and installed on your local machine (and on the PATH environment variable)

Assuming you have cloned this repo to `C:\systemsw-core\`, open your terminal and go into the directory and type in `dotnet publish`:

```bash
> cd C:\systemsw-core\
> dotnet publish
```

If this works, you should see quite a bit of activity as NPM steps in to build the UI and `dotnet` building the API. Once these are done, you should have a publish folder in the `bin\Release` (or potentially `bin\Debug`) `SystemSw-API` and `SystemSw-UI`.
These two folders are the compiled code, ready to be deployed.

Deploy these two directories to your server that will be talking to the Extron system. Modify the `appsettings.json` file in the API accordingly to ensure the correct COM port is selected when ran. Then, for EACH folder, you need to open a terminal and run the following command:
(The following assumes you have deployed to C:\published)

```bash
> cd C:\published\SystemSw-UI
> dotnet run
```

```bash
> cd C:\published\SystemSw-API
> dotnet run
```

Provided that everything has been configured correctly, both projects will start up and begin listening. You should be able to connect to the UI via:
`http://<local ip of host>:8001/`
