using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Common.Utils;
using Synchronization.Core.Interface;
using Synchronization.Core.Events;
using Synchronization.Core.Errors;

namespace Synchronization.Core.SynchronizationFlow
{
    public class UsbSynchronizer : AbstractSynchronizer
    {
        public UsbSynchronizer(ISettingsProvider settingsprovider, IUrlUtils urlUtils)
            : base(settingsprovider)
        {
            this._urlUtils = urlUtils;
            //  FlushDriversList();
        }

        #region variables

       
        private readonly IUrlUtils _urlUtils;
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

                        webClient.DownloadDataAsync(new Uri(this._urlUtils.GetUsbPushUrl(this.SettingsProvider.Settings.ClientId)));

                        while (webClient.IsBusy && !done.WaitOne(200))
                        {
                            if (this.stopRequested.WaitOne(100))
                                webClient.CancelAsync();
                        }

                        if (error != null)
                            throw error;
                    }
                }
            }
            catch (SynchronizationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SynchronizationException("Push to usb is failed", e);
            }
        }

        protected override void OnPushSupervisorCapi(SyncDirection direction)
        {
            string drive = GetDrive(); // accept driver to flush on
            if (drive == null)
                throw new SynchronizationException("Usb drivers are absend");
            try
            {
                this.stopRequested.Reset();
                usbArchive = new UsbFileArchive(drive);
                var file = usbArchive.LoadArchive();
                using (var done = new ManualResetEvent(false))
                {
                    using (var webClient = new WebClient())
                    {
                        SynchronizationException error = null;
                        webClient.UploadProgressChanged +=
                            (s, e) =>
                            {
                                var percents = e.TotalBytesToSend == 0 ? 100 :
                                    e.BytesSent * 100 / e.TotalBytesToSend;
                                var status = new SyncStatus(SyncType.Push, direction, (int)percents, null);
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
                                        error = new SynchronizationException("Push from usb is failed", e.Error);
                                    else if (cancelled)
                                        error = new CancelledSynchronizationException("Push from usb is cancelled", error);
                                    var status = new SyncStatus(SyncType.Push, direction, percents, error);
                                    OnSyncProgressChanged(new SynchronizationEvent(status));
                                }
                                finally
                                {
                                    done.Set();
                                }
                            };
                        string s1 = this._urlUtils.GetUsbPushUrl(SettingsProvider.Settings.ClientId);
                        webClient.UploadFileAsync(new Uri(s1), usbArchive.InFile);
                        while (webClient.IsBusy && !done.WaitOne(200))
                        {
                            if (this.stopRequested.WaitOne(100))
                                webClient.CancelAsync();
                        }

                        if (error != null)
                            throw error;
                    }
                }
            }
            catch (SynchronizationException)
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


                        webClient.UploadFileAsync(new Uri(this._urlUtils.GetUsbPullUrl(this.SettingsProvider.Settings.ClientId)), usbArchive.InFile);
                        
                        while (webClient.IsBusy && !done.WaitOne(200))
                        {
                            if (this.stopRequested.WaitOne(100))
                                webClient.CancelAsync();
                        }
                        
                        if (error != null)
                            throw error;
                    }
                }

            }
            catch (SynchronizationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SynchronizationException("Pull from usb is failed", e);
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
