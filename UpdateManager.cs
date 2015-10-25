using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.ComponentModel;
using System.IO.Compression;

namespace Updater
{
    public class UpdateManager
    {
        private string downloadURL { get; set; }
        private string path { get; set; }
        private string zipExtractPath { get; set; }
        private string executablePath { get; set; }
        private bool isZip { get; set; }
        private string updaterPath { get; set; }
        private WebClient webclient;
        private bool closeApp { get; set; } = false;

        private string currentVersion { get; set; }
        private string versionCheckURL { get; set; }
        private Func<string,string,bool> versionChecker { get; set; }

        public event EventHandler UpdateFileDownloaded;

        public UpdateManager(string downloadURL, string path, string updaterPath)
        {
            this.downloadURL = downloadURL;
            this.path = path;
            this.executablePath = path;
            this.updaterPath = updaterPath;
            isZip = false;
            webclient = new WebClient();
            if (Path.GetExtension(path) == ".zip")
            {
                throw new UpdateFileException("Executable path is a zip, missing zip extract path and executable file path inside zip");
            }
        }

        public UpdateManager(string downloadURL, string path, string updaterPath, string zipExtractPath, string executablePath)
        {
            this.downloadURL = downloadURL;
            this.path = path;
            this.updaterPath = updaterPath;
            this.zipExtractPath = zipExtractPath;
            if (zipExtractPath == "")
            {
                this.zipExtractPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),"TempUpdate");
            }
            if (!Directory.Exists(this.zipExtractPath))
            {
                Directory.CreateDirectory(this.zipExtractPath);
            }
            this.executablePath = executablePath;
            isZip = true;
            webclient = new WebClient();
            if (Path.GetExtension(path) != ".zip")
            {
                throw new UpdateFileException("File to download is not a zip");
            }
        }

        public void VersionCheckDownloadUpdateFile(string currentVersion, string versionCheckURL, Func<string,string,bool> versionChecker, bool async = false, bool closeApp =false)
        {
            this.currentVersion = currentVersion;
            this.versionCheckURL = versionCheckURL;
            this.versionChecker = versionChecker;
            this.closeApp = closeApp;
            webclient.DownloadStringCompleted += DownloadVersion;
            if (!async)
            {
                string n = webclient.DownloadString(versionCheckURL);
                if (versionChecker.Invoke(currentVersion, n))
                {
                    DownloadUpdateFile(false, closeApp);
                }
            }
            else
            {
                webclient.DownloadStringAsync(new Uri(versionCheckURL));
            }
        }

        private void DownloadVersion(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e != null)
            {
                if (e.Error != null)
                {
                    return;
                }
            }
            if (!versionChecker.Invoke(currentVersion, e.Result))
            {
                DownloadUpdateFile(true, closeApp);
            }
        }

        public void DownloadUpdateFile(bool async, bool closeApp)
        {
            this.closeApp = closeApp;
            if (async)
            {
                webclient.DownloadFile(downloadURL, path);
                FileDownloaded(this, null);
            }
            else
            {
                webclient.DownloadFileCompleted += FileDownloaded;
                webclient.DownloadFileAsync(new Uri(downloadURL), path);
            }
        }

        private void FileDownloaded(object sender, AsyncCompletedEventArgs e)
        {
            if (e != null)
            {
                if (e.Error != null)
                {
                    return;
                }
            }

            if (isZip)
            {
                ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Read);
                foreach(ZipArchiveEntry z in zip.Entries)
                {
                    if (File.Exists(Path.Combine(zipExtractPath, z.FullName)))
                    {
                        File.Delete(Path.Combine(zipExtractPath, z.FullName));
                    }
                    if (!Directory.Exists(Path.Combine(zipExtractPath, Path.GetDirectoryName(z.FullName))))
                    {
                        Directory.CreateDirectory(Path.Combine(zipExtractPath,Path.GetDirectoryName(z.FullName)));
                    }
                    z.ExtractToFile(Path.Combine(zipExtractPath, z.FullName));
                }
                zip.Dispose();
                File.Delete(path);
            }
            UpdateFileDownloaded?.Invoke(this, EventArgs.Empty);
            SetProcessExitEvent();
            if (closeApp)
            {
                Environment.Exit(0);
            }
        }

        private void SetProcessExitEvent()
        {
            AppDomain.CurrentDomain.ProcessExit += ProcessExit;
        }

        private void ProcessExit(object sender, EventArgs e)
        {
            File.WriteAllBytes(updaterPath, Properties.Resources.Updater);
            if (!isZip)
            {
                Process.Start(updaterPath, string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\"", "exe", Process.GetCurrentProcess().ProcessName ,System.Reflection.Assembly.GetEntryAssembly().Location, path));
            }
            else
            {
                Process.Start(updaterPath, string.Format("\"{0}\" \"{1}\" \"{2}\" \"{3}\" \"{4}\"", "zip", Process.GetCurrentProcess().ProcessName, System.Reflection.Assembly.GetEntryAssembly().Location, Path.Combine(zipExtractPath,executablePath), zipExtractPath));
            }

        }
    }
}
