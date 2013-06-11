using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Syncronization.RestUtils;
using WB.Core.Synchronization;

namespace CAPI.Android.Syncronization.Push
{
    public class RestPush
    {
        private readonly IRestUrils webExecutor;
        private const string getChunckPath = "importexport/PostPackage";
        public RestPush(IRestUrils webExecutor)
        {
            this.webExecutor = webExecutor;
        }


        public void PushChunck(string login, string password, SyncPackage chunck)
        {
            if (chunck.ItemsContainer == null || chunck.ItemsContainer.Count == 0)
                throw new InvalidOperationException("container is empty");
            webExecutor.ExcecuteRestRequest(getChunckPath, chunck.ItemsContainer[0].Content,
                                            new KeyValuePair<string, string>("login", login),
                                            new KeyValuePair<string, string>("password", password),
                                            new KeyValuePair<string, string>("aRKey",
                                                                             chunck.ItemsContainer[0].Id.ToString()));
        }
    }
}