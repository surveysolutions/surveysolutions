using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.Core.Model.ViewModel.Login;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Events.User;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;
using Ninject;
using Newtonsoft.Json;
namespace AndroidApp
{
    public class StartApplicationObject
        : MvxApplicationObject
          , IMvxStartNavigation
    {
        public void Start()
        {
           // GenerateEvents(NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus);
                RequestNavigate<LoginViewModel>();
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return false; }
        }

      
    }
}