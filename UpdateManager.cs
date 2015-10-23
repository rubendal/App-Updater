using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.ComponentModel;

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
        private bool closeApp { get; set; }

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
            this.executablePath = executablePath;
            isZip = true;
            webclient = new WebClient();
            if (Path.GetExtension(path) != ".zip")
            {
                throw new UpdateFileException("File to download is not a zip");
            }
        }

        public void VersionCheckDownloadUpdateFile(string currentVersion, string versionCheckURL, Func<string,string,bool> versionChecker, bool async = false)
        {
            this.currentVersion = currentVersion;
            this.versionCheckURL = versionCheckURL;
            this.versionChecker = versionChecker;
            webclient.DownloadStringCompleted += DownloadVersion;
            if (!async)
            {
                string n = webclient.DownloadString(versionCheckURL);
                if (versionChecker.Invoke(currentVersion, n))
                {
                    DownloadUpdateFile(false, false);
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
                DownloadUpdateFile(true, false);
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
                System.IO.Compression.ZipFile.ExtractToDirectory(path, zipExtractPath);

            }
            //UpdateFileDownloaded(this, EventArgs.Empty);
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
            if (!isZip)
            {
                Process.Start(updaterPath, string.Format("\"{0}\" \"{1}\"", System.Reflection.Assembly.GetEntryAssembly().Location, path));
            }
            else
            {
                Process.Start(updaterPath, string.Format("\"{0}\" \"{1}\" \"{2}\"", System.Reflection.Assembly.GetEntryAssembly().Location, path, Path.Combine(zipExtractPath,executablePath)));
            }

        }
    }
}
