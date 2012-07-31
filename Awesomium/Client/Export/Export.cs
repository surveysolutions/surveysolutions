using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Client.Properties;
using System.Threading;
using System.Runtime.Remoting.Messaging;
using Awesomium.Core;

namespace Client
{
    public delegate void EndOfExport();
    /// <summary>
    /// The class is responsible for completed questionaries export to plugged USB driver
    /// </summary>
    
    internal class Export
    {
        #region Nested Class

        /// <summary>
        /// Helper class to pass neccessary objects into delegate methods of parent class working with WebClient callbacks
        /// </summary>
        private class ProgressHint
        {
            public IStatusIndicator ProgressIndicator { get; private set; }
            public string ArchiveFileName { get; private set; }

            public ProgressHint(IStatusIndicator indicator, string archiveFileName)
            {
                ProgressIndicator = indicator;
                ArchiveFileName = archiveFileName;
            }
        }

        #endregion

        #region Private Members

        private IStatusIndicator pleaseWait;
        private readonly string ArchiveFileNameMask = "backup-{0}.zip";
        private WebClient webClient = new WebClient();
        private AutoResetEvent exportEnded = new AutoResetEvent(false);
        private Uri exportURL = new Uri(Settings.Default.DefaultUrl + "/Synchronizations/Export");
        private List<string> cachedDrives;
        private UsbFileArchive usbArchive;

        #endregion

        #region C-tor

        internal Export(IStatusIndicator pleaseWait)
        {
            this.webClient.Credentials = new NetworkCredential("Admin", "Admin");
            this.pleaseWait = pleaseWait;

            this.webClient.DownloadProgressChanged += (s, e) =>
            {
                var hint = e.UserState as ProgressHint;
                Debug.Assert(hint != null);

                hint.ProgressIndicator.AssignProgress(e.ProgressPercentage);
            };

            this.webClient.DownloadDataCompleted += (s, e) =>
            {
                var hint = e.UserState as ProgressHint;
                Debug.Assert(hint != null);

                try
                {
                    if (!e.Cancelled && e.Error == null)
                        usbArchive.SaveArchive(e.Result);

                    hint.ProgressIndicator.SetCompletedStatus(e.Cancelled, e.Error);

                    this.exportEnded.Set();

                    if (EndOfExport != null)
                    {
                        EndOfExport();
                    }
                
                }
                catch (Exception)
                {
                }
            };

            FlushDriversList();
        }

        #endregion

        #region Helpers

        public event EndOfExport EndOfExport;
        /// <summary>
        /// Create list of available drivers
        /// </summary>
        /// <returns></returns>
        private List<string> ReviewDriversList()
        {
            List<string> drivers = new List<string>();
            DriveInfo[] listDrives = DriveInfo.GetDrives();

            foreach (var drive in listDrives)
            {
                if (drive.DriveType == DriveType.Removable)
                    drivers.Add(drive.ToString());
            }

            return drivers;
        }

        /// <summary>
        /// Interrupt pending loading and wait for its finalization
        /// </summary>
        private void Stop()
        {
            if (this.webClient.IsBusy)
            {
                this.webClient.CancelAsync();

                this.exportEnded.WaitOne();
            }
        }

        /// <summary>
        /// Compare current drivers list with cached list and 
        /// decide what driver should be used for uploading
        /// </summary>
        /// <returns>Driver to put data on</returns>
        private string GetDrive()
        {
            List<string> currentDrivers = ReviewDriversList();

            var pluggedDrivers = currentDrivers.Except(this.cachedDrives);

            return pluggedDrivers.Any() ? pluggedDrivers.First() : null;
        }

        private void DoExport()
        {
            lock (this) // block any extra call to this method
            {
                Stop(); // stop any existent activity
                
                string drive = GetDrive(); // accept driver to flush on
                if (drive == null)
                    return;

                usbArchive = new UsbFileArchive(drive);

                this.pleaseWait.Reset();

                

                string filename = string.Format(this.ArchiveFileNameMask, DateTime.Now.ToString().Replace("/", "_"));
                filename = filename.Replace(" ", "_");
                filename = filename.Replace(":", "_");

                var archiveFilename = drive + filename;

                try
                {
                    this.exportEnded.Reset();
                    
                    //this.webClient.DownloadFileAsync(exportURL, archiveFilename, new ProgressHint(this.pleaseWait, archiveFilename));
                    this.webClient.DownloadDataAsync(exportURL, new ProgressHint(this.pleaseWait, archiveFilename));
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        #endregion

        #region Methods

        internal void ExportQuestionariesArchive()
        {
            
            new Thread(DoExport).Start(); // initialize export operation in independent thread

        }

        /// <summary>
        /// Revisit list of all removable drivers
        /// </summary>
        internal void FlushDriversList()
        {
            this.cachedDrives = ReviewDriversList();
        }

        #endregion

        internal void Interrupt()
        {
            new Thread(Stop).Start();
        }
        
    }
}
