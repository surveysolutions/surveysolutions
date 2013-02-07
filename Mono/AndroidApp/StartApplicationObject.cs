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
            GenerateEvents(NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus);
                RequestNavigate<LoginViewModel>();
        }

        public bool ApplicationCanOpenBookmarks
        {
            get { return false; }
        }

        protected void GenerateEvents(InProcessEventBus bus)
        {
            var eventStore = CapiApplication.Kernel.Get<IEventStore>() as SQLiteEventStore;
            eventStore.ClearDB();
            var stream = new UncommittedEventStream(Guid.NewGuid());
            //  var payload = new NewCompleteQuestionnaireCreated();

            #region init

            CompleteQuestionnaireDocument root = DesserializeEmbededResource<CompleteQuestionnaireDocument>("initEvent.txt");
            CompleteQuestionnaireDocument researchQ = DesserializeEmbededResource<CompleteQuestionnaireDocument>("researchDeptSurvey.txt");
            NewUserCreated userEvent = DesserializeEmbededResource<NewUserCreated>("userEvent.txt");


            var eventTempl = new UncommittedEvent(Guid.NewGuid(),
                                               root.PublicKey, 1, 0, DateTime.Now,
                                               new SnapshootLoaded()
                                               {
                                                   Template = new Snapshot(root.PublicKey, 1, root)
                                               }, new Version());

            var rEventTempl = new UncommittedEvent(Guid.NewGuid(),
                                             researchQ.PublicKey, 1, 0, DateTime.Now,
                                             new SnapshootLoaded()
                                             {
                                                 Template = new Snapshot(researchQ.PublicKey, 1, researchQ)
                                             }, new Version());

            var userEventUcmt = new UncommittedEvent(Guid.NewGuid(), userEvent.PublicKey, 1, 0, DateTime.Now, userEvent,
                                                 new Version());
            #endregion
            stream.Append(userEventUcmt);
            stream.Append(eventTempl);
            stream.Append(rEventTempl);

            eventStore.Store(stream);
            bus.Publish(userEventUcmt);
            bus.Publish(eventTempl);
            bus.Publish(rEventTempl);

        }
        protected T DesserializeEmbededResource<T>(string fileName)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            //var data = Encoding.Default.GetString("");
            string s = string.Empty;
            using (Stream streamEmbededRes = Assembly.GetExecutingAssembly()
                               .GetManifestResourceStream("AndroidApp." + fileName))
            using (StreamReader reader = new StreamReader(streamEmbededRes))
            {
                s = reader.ReadToEnd();
            }
            return JsonConvert.DeserializeObject<T>(s, settings);
        }
    }
}