using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Synchronization.Core.ClientSettings;

namespace Synchronization.Core.SynchronizationFlow
{
    public class UsbSynchronizer : AbstractSynchronizer
    {
        public UsbSynchronizer(IClientSettingsProvider clientSettingsprovider, string host, string pushAdress, string pullAdress)
            : base(clientSettingsprovider)
        {
            this._pushAdress = pushAdress;
            this._pullAdress = pullAdress;
            this._host = host;
            //  FlushDriversList();
        }

        #region variables

        protected Uri PushAdress
        {
            get { return new Uri(string.Format("{0}{1}?syncKey={2}", _host, _pushAdress, this.ClientSettingsProvider.ClientSettings.ClientId)); }
        }
        protected Uri PullAdress
        {
            get { return new Uri(string.Format("{0}{1}", _host, _pullAdress)); }
        }
        private readonly string _pushAdress;
        private readonly string _pullAdress;
        private readonly string _host;
        private WebClient webClient;
        private ManualResetEvent done;
        private readonly string ArchiveFileNameMask = "backup-{0}.zip";
        private UsbFileArchive usbArchive;

        #endregion

        #region Overrides of AbstractSynchronizer

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
                using (done = new ManualResetEvent(false))
                {
                    using (webClient = new WebClient())
                    {
                        webClient.DownloadProgressChanged +=
                            (s, e) =>
                            OnPushProgressChanged(
                                new SynchronizationEvent(
                                    (int)
                                    (((decimal) (e.TotalBytesToReceive - e.BytesReceived)/e.TotalBytesToReceive)*100)));
                        webClient.DownloadDataCompleted += (s, e) =>
                                                               {
                                                                   Exception error = e.Error;
                                                                   if (!e.Cancelled && error == null)
                                                                       usbArchive.SaveArchive(e.Result);
                                                                   OnPushProgressChanged(new SynchronizationEvent(100));
                                                                   done.Set();
                                                               };
                        webClient.DownloadDataAsync(PushAdress);
                        done.WaitOne();
                    }
                }
            }
            catch (Exception e)
            {

                throw new SynchronizationException("usb exception", e);
            }

        }
        protected override void ExecutePull()
        {
            Stop();
            string drive = GetDrive(); // accept driver to flush on
            if (drive == null)
                throw new SynchronizationException("Drivers are absend");
            try
            {
                usbArchive = new UsbFileArchive(drive);
                using (done = new ManualResetEvent(false))
                {
                    using (webClient = new WebClient())
                    {
                        webClient.UploadProgressChanged += (s, e) =>
                                                               {
                                                                   OnPullProgressChanged(
                                                                       new SynchronizationEvent(
                                                                           (int)
                                                                           (((decimal)
                                                                             (e.TotalBytesToSend - e.BytesSent)/
                                                                             e.TotalBytesToSend)*100)));
                                                               };
                        webClient.UploadFileCompleted += (s, e) => { done.Set(); };

                        webClient.UploadFileAsync(PullAdress, usbArchive.FileName);
                    }
                }

                //usbArchive

            }
            catch (Exception e)
            {

                throw new SynchronizationException("usb exception", e);
            }
        }
        #endregion

        #region utility methods

        /// <summary>
        /// Interrupt pending loading and wait for its finalization
        /// </summary>
        private void Stop()
        {
            if (webClient == null)
                return;

            if (webClient.IsBusy)
            {
                webClient.CancelAsync();
                done.Reset();
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

        #endregion

      
       
    }
}
