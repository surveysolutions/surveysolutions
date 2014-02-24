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
using Ninject.Modules;

namespace WB.Core.GenericSubdomain.Rest.Android
{
    public class RestAndroidModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IRestServiceWrapperFactory>().To<AndroidRestServiceWrapperFactory>();
            this.Bind<INetworkService>().To<AndroidNetworkService>();
        }
    }
}