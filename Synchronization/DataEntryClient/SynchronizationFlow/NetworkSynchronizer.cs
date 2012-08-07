using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace DataEntryClient.SynchronizationFlow
{
    public class NetworkSynchronizer : AbstractSynchronizer
    {
        private readonly string _url;
        public NetworkSynchronizer(string url)
        {
            this._url = url;
        }

        #region Overrides of AbstractSynchronizer

        protected override void ExecutePush()
        {
            try
            {

                WebRequest request = WebRequest.Create(this._url);
                // Set the Method property of the request to POST.
                request.Method = "POST";

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
            bool isFinished = false;
            while (isFinished)
            {
                WebRequest request = WebRequest.Create(this._url);
                // Set the Method property of the request to POST.
                request.Method = "POST";
                // Get the response.
                using (WebResponse response = request.GetResponse())
                {
                    // Get the stream containing content returned by the server.
                    var dataStream = response.GetResponseStream();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader(dataStream);

                    isFinished = bool.Parse(reader.ReadToEnd());

                    // Clean up the streams.
                    reader.Close();
                    dataStream.Close();
                    response.Close();
                }
            }

        }

        protected override void ExecutePull()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
