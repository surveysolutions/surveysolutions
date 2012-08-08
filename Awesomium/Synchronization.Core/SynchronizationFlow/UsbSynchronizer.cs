using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace Synchronization.Core.SynchronizationFlow
{
    public class UsbSynchronizer : AbstractSynchronizer
    {
        private readonly string _exportURL;
        public UsbSynchronizer( string exportURL)
        {
            this._exportURL = exportURL;
          //  FlushDriversList();
        }

        #region Overrides of AbstractSynchronizer

        private WebClient webClient;
        protected override void ExecutePush()
        {
            Stop();

            string drive = GetDrive(); // accept driver to flush on
            if (drive == null)
                throw new SynchronizationException("Driavers are absend");
            try
            {
                usbArchive = new UsbFileArchive(drive);

                /*  string filename = string.Format(this.ArchiveFileNameMask, DateTime.Now.ToString().Replace("/", "_"));
                  filename = filename.Replace(" ", "_");
                  filename = filename.Replace(":", "_");

                  var archiveFilename = drive + filename;*/
                using (webClient = new WebClient())
                {
                    bool isFinished = false;

                    
                    webClient.DownloadProgressChanged +=
                        (s, e) =>
                        OnPushProgressChanged(
                            new SynchronizationEvent((int)(((decimal)(e.TotalBytesToReceive - e.BytesReceived) / e.TotalBytesToReceive) * 100)));
                    webClient.DownloadDataCompleted += (s, e) =>
                                                           {
                                                               Exception error = e.Error;
                                                               if (!e.Cancelled && error == null)
                                                                   usbArchive.SaveArchive(e.Result);
                                                               OnPushProgressChanged(new SynchronizationEvent(100));
                                                               isFinished = true;
                                                           };
                    webClient.DownloadDataAsync(new Uri(_exportURL));
                    while (!isFinished)
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception e)
            {

                throw new SynchronizationException("usb exception", e);
            }

        }

        /// <summary>
        /// Interrupt pending loading and wait for its finalization
        /// </summary>
        private void Stop()
        {
            if(webClient==null)
                return;
            
            if (webClient.IsBusy)
            {
                webClient.CancelAsync();
               // this.exportEnded.WaitOne();
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

           /* var pluggedDrivers = currentDrivers.Except(this.cachedDrives);

            return pluggedDrivers.Any() ? pluggedDrivers.First() : null;*/
            return currentDrivers.FirstOrDefault();
        }
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
        private readonly string ArchiveFileNameMask = "backup-{0}.zip";
        //private WebClient webClient = new WebClient();
       
     //   private List<string> cachedDrives;
        private UsbFileArchive usbArchive;
       /* /// <summary>
        /// Revisit list of all removable drivers
        /// </summary>
        internal void FlushDriversList()
        {
            this.cachedDrives = ReviewDriversList();
        }*/
        protected override void ExecutePull()
        {
            string drive = GetDrive(); // accept driver to flush on
            if (drive == null)
                throw new SynchronizationException("Drivers are absend");
            try
            {
                usbArchive = new UsbFileArchive(drive);
                SubmitFile(usbArchive);

                //usbArchive

            }
            catch (Exception e)
            {

                throw new SynchronizationException("usb exception", e);
            }
        }
        protected void SubmitFile(UsbFileArchive usbArchive)
        {
            ManualResetEvent done = new ManualResetEvent(false);
            WebClient client = new WebClient();
            client.UploadProgressChanged += (s, e) =>
                                                {
                                                    OnPullProgressChanged(new SynchronizationEvent((int)(((decimal)(e.TotalBytesToSend - e.BytesSent) / e.TotalBytesToSend) * 100)));
                                                };
            client.UploadFileCompleted += (s, e) => { done.Set(); };
            
            client.UploadFileAsync(new Uri("http://localhost:8083/Synchronizations/Import"), usbArchive.FileName);
            done.WaitOne();
        }

        #endregion
    }
}
