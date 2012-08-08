using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Synchronization.Core.SynchronizationFlow
{
    public class NetworkSynchronizer : AbstractSynchronizer
    {
        private readonly string _host;
        private readonly string _pushAdress;
        private readonly string _pushCheckStateAdress;
        private readonly string _pullAdress;
        public NetworkSynchronizer(string host, string pushAdress, string pushCheckStateAdress, string pullAdress)
        {
            this._host = host;
            this._pullAdress = pullAdress;
            this._pushAdress = pushAdress;
            this._pushCheckStateAdress = pushCheckStateAdress;
        }

        protected Uri PushAdress
        {
            get { return new Uri(string.Format("{0}{1}?url={2}",_host , _pushAdress,_pullAdress)); }
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

                    WaitForEndProcess(Guid.Parse(responseFromServer));
                    // Clean up the streams.
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                }
            }
            catch (Exception e)
            {
                new SynchronizationException("network exception", e);
            }
        }
        protected void WaitForEndProcess(Guid processid)
        {
            int isFinished = 0;
            while (isFinished!=100)
            {
                WebRequest request = WebRequest.Create(string.Format("{0}?id={1}",PushCheckStateAdress, processid));
                // Set the Method property of the request to POST.
                request.Method = "GET";
                // Get the response.
                using (WebResponse response = request.GetResponse())
                {
                    // Get the stream containing content returned by the server.
                    var dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);

                    isFinished = int.Parse(reader.ReadToEnd());

                    // Clean up the streams.
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                    OnPushProgressChanged(new SynchronizationEvent(isFinished));
                }
                Thread.Sleep(1000);
            }

        }

        protected override void ExecutePull()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
