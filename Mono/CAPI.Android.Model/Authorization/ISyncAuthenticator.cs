using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CAPI.Android.Core.Model.Authorization
{
    public interface ISyncAuthenticator
    {
        SyncCredentials RequestCredentials();

        event RequestCredentialsCallBack RequestCredentialsCallback;
    }

    public delegate SyncCredentials? RequestCredentialsCallBack(object sender);


}