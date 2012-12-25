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
using System.ComponentModel;

namespace Synchronization.Core.SynchronizationFlow
{
    public class UsbSynchronizer : AbstractSynchronizer
    {
        public UsbSynchronizer(ISettingsProvider settingsprovider, IRequestProcessor requestProcessor, IUrlUtils urlUtils, IUsbProvider usbProvider)
            : base(settingsprovider, requestProcessor, urlUtils)
        {
            this.usbProvider = usbProvider;
        }

        #region Private Members

        private readonly IUsbProvider usbProvider;
        private string lastUsbArchiveName;
        private ManualResetEvent stopUsbAccessRequested = new ManualResetEvent(false);

        #endregion

        #region Overrides of AbstractSynchronizer

        protected override void OnWaitForEndProcess(Action<SynchronizationEventArgs> eventRiser, SyncType syncType, SyncDirection direction)
        {
            if (syncType == SyncType.Push)
                return;

            base.OnWaitForEndProcess(eventRiser, syncType, direction);
        }

        protected override Guid OnPush(SyncDirection direction)
        {
            var drive = GetDrive(); // accept driver to flash on

            this.stopUsbAccessRequested.Reset();

            try
            {
                var usbArchive = new UsbFileArchive(drive, direction == SyncDirection.Down);
                var usbArchiveName = usbArchive.ArchiveFileName;

                using (var done = new AutoResetEvent(false))
                {
                    using (var webClient = new WebClient())
                    {
                        ServiceException error = null;

                        webClient.DownloadProgressChanged +=
                            (s, e) =>
                            {
                                var percents = e.TotalBytesToReceive == 0 ? 100 :
                                    e.BytesReceived * 100 / e.TotalBytesToReceive;

                                var status = new SyncStatus(SyncType.Push, direction, (int)percents, null, "Downloading data ..");

                                OnSyncProgressChanged(new SynchronizationEventArgs(status));
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
                                        error = new CancelledServiceException("Push to usb is cancelled", error);
                                    else if (errornous)
                                        error = new SynchronizationException("Push to usb is failed", e.Error);
                                    else
                                    {
                                        usbArchive.SaveArchive(e.Result);
                                        this.lastUsbArchiveName = usbArchiveName;
                                    }

                                    var status = new SyncStatus(SyncType.Push, direction, percents, error, error == null ? "Completed .." : null);

                                    OnSyncProgressChanged(new SynchronizationEventArgs(status));
                                }
                                finally
                                {
                                    done.Set();
                                }
                            };

                        var url = new Uri(this.UrlUtils.GetUsbPushUrl(this.SettingsProvider.Settings.ClientId));

                        webClient.DownloadDataAsync(url);

                        while (webClient.IsBusy && !done.WaitOne(200))
                        {
                            if (this.stopUsbAccessRequested.WaitOne(100))
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
            catch (ServiceException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SynchronizationException("Push to usb is failed", e);
            }

            return Guid.Empty;
        }

        /// <summary>
        /// Getting USB file and uploading it to web service
        /// </summary>
        /// <param name="direction"></param>
        protected override Guid OnPull(SyncDirection direction)
        {
            var drive = GetDrive(); // accept driver to flash on
            Guid syncProcessId = Guid.Empty;

            this.stopUsbAccessRequested.Reset();
 
            try
            {
                var usbArchive = new UsbFileArchive(drive, direction == SyncDirection.Down);
                var usbArchiveName = usbArchive.ArchiveFileName;

                this.lastUsbArchiveName = string.Empty;

                using (var done = new ManualResetEvent(false))
                {
                    using (var webClient = new WebClient())
                    {
                        ServiceException error = null;

                        webClient.UploadProgressChanged +=
                            (s, e) =>
                            {
                                var percents = e.TotalBytesToSend == 0 ? 100 :
                                    e.BytesSent * 100 / e.TotalBytesToSend;

                                var status = new SyncStatus(SyncType.Pull, direction, (int)percents, null, "Uploading data ..");

                                OnSyncProgressChanged(new SynchronizationEventArgs(status));
                            };

                        webClient.UploadFileCompleted +=
                            (s, e) =>
                            {
                                bool errornous = e.Error != null;
                                bool cancelled = e.Cancelled;
                                int percents = errornous || cancelled ? 0 : 100;

                                try
                                {
                                    syncProcessId = new Guid(System.Text.Encoding.UTF8.GetString(e.Result));
                                }
                                catch
                                {
                                }

                                try
                                {
                                    this.lastUsbArchiveName = string.Empty;

                                    if (cancelled)
                                        error = new CancelledServiceException("Pull from usb is cancelled", error);
                                    else if (errornous)
                                        error = new SynchronizationException("Pull from usb is failed", e.Error);
                                    else
                                        this.lastUsbArchiveName = usbArchiveName;

                                    var status = new SyncStatus(SyncType.Push, direction, percents / 2, error, error == null ? "Completed .." : null);

                                    OnSyncProgressChanged(new SynchronizationEventArgs(status));
                                }
                                finally
                                {
                                    done.Set();
                                }
                            };


                        var url = new Uri(this.UrlUtils.GetUsbPullUrl(this.SettingsProvider.Settings.ClientId));

                        webClient.UploadFileAsync(url, usbArchiveName);

                        while (webClient.IsBusy && !done.WaitOne(200))
                        {
                            if (this.stopUsbAccessRequested.WaitOne(100))
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
            catch (ServiceException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new SynchronizationException("Pull from usb is failed", e);
            }

            return syncProcessId;
        }

        protected override void OnStop()
        {
            this.stopUsbAccessRequested.Set();
 
            base.OnStop(); // stop web processing as well
        }

        public override string GetSuccessMessage(SyncType syncAction, SyncDirection direction)
        {
            return string.Format("Usb synchronization {0} is successful with file {1}", syncAction, this.lastUsbArchiveName);
        }

        protected override IList<ServiceException> OnCheckSyncIssues(SyncType syncType, SyncDirection direction)
        {
            try
            {
                var drive = GetDrive(); // only check if usb driver available and choosen
                return null;
            }
            catch (Exception ex)
            {
                return new List<ServiceException>() { new UsbNotAccessableException(ex.Message) };
            }
        }

        protected override bool OnUpdateStatus()
        {
            return true;
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
                    throw new UsbNotChoozenException();
                else
                    throw new UsbNotPluggedException();

            return drive;
        }

        #endregion
    }
}
