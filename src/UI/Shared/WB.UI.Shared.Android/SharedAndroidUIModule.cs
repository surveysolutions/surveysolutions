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
using WB.UI.Shared.Android.Implementations.Network;
using WB.UI.Shared.Android.Network;

namespace WB.UI.Shared.Android
{
    public class SharedAndroidUiModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ICapiNetworkService>().To<AndroidNetworkService>();
        }
    }
}