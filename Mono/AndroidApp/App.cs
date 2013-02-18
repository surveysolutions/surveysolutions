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
using Cirrious.MvvmCross.Application;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;

namespace AndroidApp
{
    public class App
         : MvxApplication
         , IMvxServiceProducer
    {
        public App()
        {
            this.RegisterServiceInstance<IMvxStartNavigation>(new StartApplicationObject());
        }
    }
}