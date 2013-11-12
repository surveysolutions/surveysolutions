using System;
using System.Threading;
using RestSharp;
using WB.UI.Shared.Android.RestUtils;

namespace WB.UI.Capi.DataCollection.Syncronization.Push
{
    public class RestPush
    {
        private readonly IRestUrils webExecutor;
        private const string getChunckPath = "sync/PostPackage";
        public RestPush(IRestUrils webExecutor)
        {
            this.webExecutor = webExecutor;
        }


        public void PushChunck(string login, string password, string content, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(content))
                throw new InvalidOperationException("container is empty");

            try
            {
                var result = this.webExecutor.ExcecuteRestRequestAsync<bool>(getChunckPath, ct,
                    content, 
                    new HttpBasicAuthenticator(login, password));

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