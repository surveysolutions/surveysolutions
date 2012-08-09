using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Synchronization.Core.ClientSettings;

namespace Synchronization.Core.SynchronizationFlow
{
    public class NetworkSynchronizer : AbstractSynchronizer
    {
        #region variables

        private readonly string _host;
        private readonly string _pushAdress;
        private readonly string _pullAdress;
        private readonly string _pushCheckStateAdress;
        private readonly string _endPointAdressAdress;
        #endregion


        public NetworkSynchronizer(IClientSettingsProvider clientSettingsprovider, string host, string pushAdress, string pullAdress, string pushCheckStateAdress, string endPointAdressAdress)
            : base(clientSettingsprovider)
        {
            this._host = host;
            this._endPointAdressAdress = endPointAdressAdress;
            this._pushAdress = pushAdress;
            this._pullAdress = pullAdress;
            this._pushCheckStateAdress = pushCheckStateAdress;
        }

        protected Uri PushAdress
        {
            get { return new Uri(string.Format("{0}{1}?url={2}&syncKey={3}", _host, _pushAdress, _endPointAdressAdress, this.ClientSettingsProvider.ClientSettings.ClientId)); }
        }
        protected Uri PullAdress
        {
            get { return new Uri(string.Format("{0}{1}?url={2}&syncKey={3}", _host, _pullAdress, _endPointAdressAdress, this.ClientSettingsProvider.ClientSettings.ClientId)); }
        }
        protected Uri PushCheckStateAdress
        {
            get { return new Uri(_host + _pushCheckStateAdress); }
        }

        #region Overrides of AbstractSynchronizer

        protected override void ExecutePush()
        {
            try
            {
                WebRequest request = WebRequest.Create(PushAdress);
                request.Method = "GET";
                // Get the response.
                using (WebResponse response = request.GetResponse())
                {
                    // Get the stream containing content returned by the server.
                    var dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();

                    WaitForEndProcess(Guid.Parse(responseFromServer),OnPushProgressChanged);
                    // Clean up the streams.
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                }
            }
            catch (Exception e)
            {
                throw new SynchronizationException("network exception", e);
            }
        }

        protected override void ExecutePull()
        {
            try
            {
                WebRequest request = WebRequest.Create(PullAdress);
                request.Method = "GET";
                // Get the response.
                using (WebResponse response = request.GetResponse())
                {
                    // Get the stream containing content returned by the server.
                    var dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.
                    string responseFromServer = reader.ReadToEnd();

                    WaitForEndProcess(Guid.Parse(responseFromServer), OnPullProgressChanged);
                    // Clean up the streams.
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                }
            }
            catch (Exception e)
            {
                throw new SynchronizationException("network exception", e);
            }
        }

        #endregion

        #region utility methods

        protected void WaitForEndProcess(Guid processid, Action<SynchronizationEvent> eventRiser)
        {
            int percentage = 0;
            while (percentage != 100)
            {
                WebRequest request = WebRequest.Create(string.Format("{0}?id={1}", PushCheckStateAdress, processid));
                // Set the Method property of the request to POST.
                request.Method = "GET";
                // Get the response.
                using (WebResponse response = request.GetResponse())
                {
                    // Get the stream containing content returned by the server.
                    var dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);

                    percentage = int.Parse(reader.ReadToEnd());

                    // Clean up the streams.
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                    if (percentage < 0)
                        throw new SynchronizationException("network synchronization is failed");
                    eventRiser(new SynchronizationEvent(percentage));
                }
                Thread.Sleep(1000);
            }

        }

        #endregion

    }
}
