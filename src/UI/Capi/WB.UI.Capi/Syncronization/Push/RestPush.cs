using System;
using System.Threading;
using RestSharp;
using WB.Core.GenericSubdomain.Rest;

namespace WB.UI.Capi.Syncronization.Push
{
    public class RestPush
    {
        private readonly IRestServiceWrapper webExecutor;
        private const string getChunckPath = "sync/PostPackage";
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
                var result = this.webExecutor.ExecuteRestRequestAsync<bool>(getChunckPath, ct,
                    content, login, password, null);

                if (!result)
                    throw new SynchronizationException("Push was failed. Try again later.");
            }
            catch (RestException)
            {
                throw new SynchronizationException("Data sending was canceled");
            }

        }
    }
}