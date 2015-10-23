# App Updater
.NET library to make easy application updates

### Note
Development in progress, not fully functional and missing stuff

## UpdateManager
Instead of creating a WebClient/WebRequest to check newest version and download a file with a lot of code *UpdateManager* class makes it easier

```
UpdateManager um = new UpdateManager("url.exe", "executablePath", "updaterPath"); 
// For zip files
UpdateManager um = new UpdateManager("url.zip", "zipPath", "updaterPath", "zipExtractionPath", "executablePathInsideZip"); 
```

Updater console app is included in the lib, extract it and use its file path in the "updaterPath" field

### Check for updates and download update
Checking for updates and download an update if there is a new version can be done with one method

```
um.VersionCheckDownloadUpdateFile("currentVersion", "urlWithCurrentAppVersion", (string v, string v2) => s != s2, [bool async=false]);
```

This method accepts a *Func`<`string,string,bool`>`* to evaluate versions based on a custom format

```
// Func<string,string,bool> Examples using txt files
// Simple txt file containing only the new version
(string v, string v2) => s != s2
// txt file containing the version in the first line and then something like a changelog 
(string v, string v2) => s != s2.Split('\n')[0].Trim()
```

It also supports asynchronous downloading of files

Once the download finishes closing the application will start the updater if specified with updaterPath
