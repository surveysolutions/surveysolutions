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
        public UsbSynchronizer(ISettingsProvider settingsprovider, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base(settingsprovider)
        {
            this.urlUtils = urlUtils;
            this.usbProvider = usbProvider;
        }

        #region Private Members

        private readonly IUsbProvider usbProvider;
        private IUrlUtils urlUtils;
        private string lastUsbArchiveName;
        private AutoResetEvent stopRequested = new AutoResetEvent(false);

        #endregion

        #region Overrides of AbstractSynchronizer

        protected override void OnPush(SyncDirection direction)
        {
            var drive = GetDrive(); // accept driver to flush on

            try
            {
                this.stopRequested.Reset();

                var usbArchive = new UsbFileArchive(drive, direction == SyncDirection.Down);
                var usbArchiveName = usbArchive.ArchiveFileName;

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
                                    this.lastUsbArchiveName = string.Empty;

                                    if (cancelled)
                                        error = new CancelledSynchronizationException("Push to usb is cancelled", error);
                                    else if (errornous)
                                        error = new SynchronizationException("Push to usb is failed", e.Error);
                                    else
                                    {
                                        usbArchive.SaveArchive(e.Result);
                                        this.lastUsbArchiveName = usbArchiveName;
                                    }

                                    var status = new SyncStatus(SyncType.Push, direction, percents, error);

                                    OnSyncProgressChanged(new SynchronizationEvent(status));
                                }
                                finally
                                {
                                    done.Set();
                                }
                            };

                        var url = new Uri(this.urlUtils.GetUsbPushUrl(this.SettingsProvider.Settings.ClientId));

                        webClient.DownloadDataAsync(url);

                        while (webClient.IsBusy && !done.WaitOne(200))
                        {
                            if (this.stopRequested.WaitOne(100))
                            {
                                webClient.CancelAsync();
                                break;
                            }
                        }

                        done.WaitOne(10000);

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

        /// <summary>
        /// Getting USB file and uploading it to web service
        /// </summary>
        /// <param name="direction"></param>
        protected override void OnPull(SyncDirection direction)
        {
            var drive = GetDrive(); // accept driver to flush on

            try
            {
                var usbArchive = new UsbFileArchive(drive, direction == SyncDirection.Down);
                var usbArchiveName = usbArchive.ArchiveFileName;

                this.lastUsbArchiveName = string.Empty;
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
                                    e.BytesSent * 100 / e.TotalBytesToSend;

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
                                    this.lastUsbArchiveName = string.Empty;

                                    if (cancelled)
                                        error = new CancelledSynchronizationException("Pull from usb is cancelled", error);
                                    else if (errornous)
                                        error = new SynchronizationException("Pull from usb is failed", e.Error);
                                    else
                                        this.lastUsbArchiveName = usbArchiveName;

                                    var status = new SyncStatus(SyncType.Push, direction, percents, error);

                                    OnSyncProgressChanged(new SynchronizationEvent(status));
                                }
                                finally
                                {
                                    done.Set();
                                }
                            };


                        var url = new Uri(this.urlUtils.GetUsbPullUrl(this.SettingsProvider.Settings.ClientId));

                        webClient.UploadFileAsync(url, usbArchiveName);

                        while (webClient.IsBusy && !done.WaitOne(200))
                        {
                            if (this.stopRequested.WaitOne(100))
                            {
                                webClient.CancelAsync();
                                break;
                            }
                        }

                        done.WaitOne(10000);

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

        public override string BuildSuccessMessage(SyncType syncAction, SyncDirection direction)
        {
            return string.Format("Usb {0} is successful with file {1}", syncAction, this.lastUsbArchiveName);
        }

        protected override bool OnCheckIsPushPossible(SyncDirection direction)
        {
            return OnCheckIsPullPossible(direction);
        }

        protected override bool OnCheckIsPullPossible(SyncDirection direction)
        {
            try
            {
                var drive = GetDrive(); // only check if usb driver available and choozen
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region utility methods

        /// <summary>
        /// Compare current drivers list with cached list and 
        /// decide what driver should be used for uploading
        /// </summary>
        /// <returns>Driver to put data on</returns>
        private DriveInfo GetDrive()
        {
            var drive = this.usbProvider.ActiveUsb;

            if (drive == null)
                if (this.usbProvider.IsAnyAvailable)
                    throw new SynchronizationException("Usb flush memory device has not been choozen");
                else
                    throw new SynchronizationException("Usb flush memory device has not been plugged");

            return drive;
        }

        #endregion
    }
}
