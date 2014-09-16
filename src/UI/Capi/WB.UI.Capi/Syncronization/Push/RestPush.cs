using System;
using System.Collections.Generic;
using System.Threading;
using RestSharp;
using WB.Core.GenericSubdomains.Rest;
using WB.Core.GenericSubdomains.Utils;

namespace WB.UI.Capi.Syncronization.Push
{
    public class RestPush
    {
        private readonly IRestServiceWrapper webExecutor;
        private const string PostPackagePath = "sync/PostPackage";
        private const string PostFilePath = "sync/PostFile";
        public RestPush(IRestServiceWrapper webExecutor)
        {
            this.webExecutor = webExecutor;
        }


        public void PushChunck(string login, string password, string content, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(content))
                throw new InvalidOperationException("container is empty");

            try
            {
                bool result = this.webExecutor.ExecuteRestRequestAsync<bool>(PostPackagePath, ct,
                    content, login, password, null);

                if (!result)
                    throw new SynchronizationException("Push was failed. Try again later.");
            }
            catch (RestException)
            {
                throw new SynchronizationException("Data sending was canceled");
            }

        }

        public void PushBinary(string login, string password, byte[] data, string fileName, Guid interviewId, CancellationToken ct)
        {
            if (data==null)
                throw new InvalidOperationException("data is empty");

            try
            {
                bool result = this.webExecutor.ExecuteRestRequestAsync<bool>(PostFilePath, ct,
                    System.Text.Encoding.Default.GetString(data), login, password, null,
                    new KeyValuePair<string, string>("interviewId", interviewId.FormatGuid()),
                    new KeyValuePair<string, string>("pictureFileName", fileName));

                if (!result)
                    throw new SynchronizationException("Push binary data was failed. Try again later.");
            }
            catch (RestException)
            {
                throw new SynchronizationException("Data sending was canceled");
            }
        }
    }
}