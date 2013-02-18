using System;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using Android.Runtime;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Android.OS;
using AndroidApp.Injections;
using AndroidApp.ViewModel.Input;
using AndroidApp.ViewModel.Model;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Cirrious.MvvmCross.Binding.Droid.Simple;
using Core.CAPI.Views.Grouped;
using Main.Core;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.View;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using Newtonsoft.Json;
using Ninject;
using Android.Graphics;
namespace AndroidApp
{
    [Activity(Label = "CAPI", MainLauncher = true, Icon = "@drawable/capi")]
    public class Activity1 : MvxSimpleBindingActivity<DashboardModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            ViewModel =
                CapiApplication.LoadView<DashboardInput, DashboardModel>(new DashboardInput());
            SetContentView(Resource.Layout.Main);

        }
    }
}

