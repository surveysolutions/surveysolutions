using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Synchronization.Core.Interface;
using Synchronization.Core.Events;
using Synchronization.Core.Errors;

namespace Synchronization.Core.SynchronizationFlow
{
    public class UsbSynchronizer : AbstractSynchronizer
    {
        public UsbSynchronizer(ISettingsProvider settingsprovider, string host, string pushAdress, string pullAdress)
            : base(settingsprovider)
        {
            this._pushAdress = pushAdress;
            this._pullAdress = pullAdress;
            this._host = host;
            //  FlushDriversList();
        }

        #region variables

        protected Uri PushAdress
        {
            get { return new Uri(string.Format("{0}{1}?syncKey={2}", _host, _pushAdress, this.SettingsProvider.Settings.ClientId)); }
        }

        protected Uri PullAdress
        {
            get { return new Uri(string.Format("{0}{1}", _host, _pullAdress)); }
        }

        private readonly string _pushAdress;
        private readonly string _pullAdress;
        private readonly string _host;
        private readonly string ArchiveFileNameMask = "backup-{0}.zip";

        private UsbFileArchive usbArchive;
        private AutoResetEvent stopRequested = new AutoResetEvent(false);

        #endregion

        public string OutFilePath
        {
            get
            {
                if (usbArchive == null)
                    return string.Empty;
                return usbArchive.OutFile;
            }
        }

        public string InFilePath
        {
            get
            {
                if (usbArchive == null)
                    return string.Empty;
                return usbArchive.InFile;
            }
        }

        #region Overrides of AbstractSynchronizer

        protected override void OnPush(SyncDirection direction)
        {
            string drive = GetDrive(); // accept driver to flush on
            if (drive == null)
                throw new SynchronizationException("Usb drivers are absend");

            try
            {
                this.stopRequested.Reset();

                usbArchive = new UsbFileArchive(drive);

                using (var done = new AutoResetEvent(false))
                {
                    using (var webClient = new WebClient())
                    {
                        SynchronizationException error = null;

                        webClient.DownloadProgressChanged +=
                            (s, e) =>
                            {
                                var percents = e.TotalBytesToReceive == 0 ? 100 :
                                    e.BytesReceived * 100 / e.TotalBytesToReceive;

                                var status = new SyncStatus(SyncType.Push, direction, (int)percents, null);

                                OnSyncProgressChanged(new SynchronizationEvent(status));
                            };

                        webClient.DownloadDataCompleted +=
                            (s, e) =>
                            {
                                bool errornous = e.Error != null;
                                bool cancelled = e.Cancelled;
                                int percents = errornous || cancelled ? 0 : 100;

                                try
                                {
                                    if (errornous)
                                        error = new SynchronizationException("Push to usb is failed", e.Error);
                                    else if (cancelled)
                                        error = new CancelledSynchronizationException("Push to usb is cancelled", error);
                                    else
                                        usbArchive.SaveArchive(e.Result);

                                    var status = new SyncStatus(SyncType.Push, direction, percents, error);

                                    OnSyncProgressChanged(new SynchronizationEvent(status));
                                }
                                finally
                                {
                                    done.Set();
                                }
                            };

                        webClient.DownloadDataAsync(PushAdress);

                        while (!done.WaitOne(200))
                        {
                            if (this.stopRequested.WaitOne(100))
                                webClient.CancelAsync();
                        }

                        if (error != null)
                            throw error;
                    }
                }
            }
            catch (CancelledSynchronizationException ex)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SynchronizationException("Push to usb is failed", e);
            }
        }

        protected override void OnPull(SyncDirection direction)
        {
            string drive = GetDrive(); // accept driver to flush on
            if (drive == null)
                throw new SynchronizationException("Usb drivers are absend");

            try
            {
                usbArchive = new UsbFileArchive(drive);

                usbArchive.LoadArchive();

                using (var done = new ManualResetEvent(false))
                {
                    using (var webClient = new WebClient())
                    {
                        SynchronizationException error = null;

                        webClient.UploadProgressChanged +=
                            (s, e) =>
                            {
                                var percents = e.TotalBytesToSend == 0 ? 100 :
                                    e.BytesSent * 100 /  e.TotalBytesToSend;

                                var status = new SyncStatus(SyncType.Pull, direction, (int)percents, null);

                                OnSyncProgressChanged(new SynchronizationEvent(status));
                            };

                        webClient.UploadFileCompleted +=
                            (s, e) =>
                            {
                                bool errornous = e.Error != null;
                                bool cancelled = e.Cancelled;
                                int percents = errornous || cancelled ? 0 : 100;

                                try
                                {
                                    if (errornous)
                                        error = new SynchronizationException("Pull from usb is failed", e.Error);
                                    else if (cancelled)
                                        error = new CancelledSynchronizationException("Pull from usb is cancelled",  error);

                                    var status = new SyncStatus(SyncType.Push, direction, percents, error);

                                    OnSyncProgressChanged(new SynchronizationEvent(status));
                                }
                                finally
                                {
                                    done.Set();
                                }
                            };


                        webClient.UploadFileAsync(PullAdress, usbArchive.InFile);
                        
                        while (done.WaitOne(200))
                        {
                            if (this.stopRequested.WaitOne(100))
                                webClient.CancelAsync();
                        }
                        
                        if (error != null)
                            throw error;
                    }
                }

            }
            catch (CancelledSynchronizationException ex)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SynchronizationException("Pull to usb is failed", e);
            }
        }

        /// <summary>
        /// Interrupt pending loading and wait for its finalization
        /// </summary>
        protected override void OnStop()
        {
            this.stopRequested.Set();
        }

        #endregion

        #region utility methods

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

        public override string BuildSuccessMessage(SyncType syncAction, SyncDirection direction)
        {
            return string.Format("Usb {0} is successful with file {1}", syncAction,
                               syncAction == SyncType.Pull ? InFilePath : OutFilePath);
        }
    }
}
