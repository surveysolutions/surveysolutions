using System;
using System.Collections.Generic;
using System.Threading;
using CAPI.Android.Syncronization.RestUtils;
using Newtonsoft.Json;
using RestSharp;
using WB.Core.SharedKernel.Structures.Synchronization;
namespace CAPI.Android.Syncronization.Push
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
                var result = webExecutor.ExcecuteRestRequestAsync<bool>(getChunckPath, ct,
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