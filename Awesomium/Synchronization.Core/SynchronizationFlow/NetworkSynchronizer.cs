using System;
using System.IO;
using System.Net;
using System.Text;

namespace Synchronization.Core.SynchronizationFlow
{
    public class NetworkSynchronizer : AbstractSynchronizer
    {
        private readonly string _host;
        private readonly string _pushAdress;
        private readonly string _pushReciverAdress;
        private readonly string _pushCheckStateAdress;
        private readonly string _pullAdress;
        public NetworkSynchronizer(string host, string pushAdress, string pushReciverAdress, string pushCheckStateAdress, string pullAdress)
        {
            this._host = host;
            this._pullAdress = pullAdress;
            this._pushReciverAdress = pushReciverAdress;
            this._pushAdress = pushAdress;
            this._pushCheckStateAdress = pushCheckStateAdress;
        }

        protected Uri PullAdress
        {
            get { return new Uri(_host + _pullAdress); }
        }
        protected Uri PushAdress
        {
            get { return new Uri(_host + _pushAdress); }
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
                // Set the Method property of the request to POST.
                request.Method = "POST";
                
                byte[] postDataStream = Encoding.UTF8.GetBytes(string.Format("url={0}", this._pushReciverAdress));
                request.ContentLength = postDataStream.Length;
                using (Stream newStream = request.GetRequestStream())
                {
                    // Send the data.
                    newStream.Write(postDataStream, 0, postDataStream.Length);
                    newStream.Close();
                }
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
                WebRequest request = WebRequest.Create(PushCheckStateAdress);
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
