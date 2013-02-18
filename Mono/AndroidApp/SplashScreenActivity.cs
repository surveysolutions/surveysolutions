using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using Android.Provider;
using Android.Widget;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Cirrious.MvvmCross.Droid.Views;
using Main.Core.Documents;
using Main.Core.Events.User;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;

using Ninject;

namespace AndroidApp
{
    [Activity(Label = "CAPI", MainLauncher = true, NoHistory = true, Icon = "@drawable/capi")]
    public class SplashScreenActivity : MvxBaseSplashScreenActivity
    {
        public SplashScreenActivity()
            : base(Resource.Layout.SplashScreen)
        {
        }

      
    }
}