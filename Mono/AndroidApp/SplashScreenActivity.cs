using System.IO;
using System.Reflection;
using System.Threading;
using Android.App;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.Droid.Views;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;


namespace AndroidApp
{
    [Activity(Label = "CAPI", MainLauncher = true, NoHistory = true, Icon = "@drawable/capi")]
    public class SplashScreenActivity : MvxBaseSplashScreenActivity
    {
        public SplashScreenActivity()
            : base(Resource.Layout.SplashScreen)
        {
        }
        protected override void TriggerFirstNavigate()
        {
            CapiApplication.GenerateEvents();
            base.TriggerFirstNavigate();
        }

    }
}