using System;
using System.Net;

namespace Synchronization.Core.SynchronizationFlow
{
    public class UsbSynchronizer : AbstractSynchronizer
    {
        private readonly string _exportURL;
        public UsbSynchronizer( string exportURL)
        {
            this._exportURL = exportURL;
        }

        #region Overrides of AbstractSynchronizer

        protected override void ExecutePush()
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadDataAsync(new Uri(_exportURL));
                }
            }
            catch (Exception e)
            {

                new SynchronizationException("usb exception", e);
            }

        }

        protected override void ExecutePull()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
