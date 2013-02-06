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
using AndroidApp.Core.Model.Authorization;
using AndroidApp.Core.Model.EventHandlers;
using AndroidApp.Core.Model.ProjectionStorage;
using AndroidApp.Core.Model.ViewModel.Dashboard;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using AndroidApp.Injections;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.Events.User;
using Main.Core.View;
using Main.Core.View.User;
using Main.DenormalizerStorage;
using Ncqrs;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;
using Newtonsoft.Json;
using Ninject;

namespace AndroidApp
{
    [Application]
    public class CapiApplication : Application
    {
        public static IKernel Kernel { get;private set; }
        public static TOutput LoadView<TInput, TOutput>(TInput input)
        {
            return Kernel.Get<IViewRepository>().Load<TInput, TOutput>(input);
        }

        public static ICommandService CommandService
        {
            get { return NcqrsEnvironment.Get<ICommandService>(); }
        }

        public static IProjectionStorage ProjectionStorage
        {
            get { return Kernel.Get<IProjectionStorage>(); }
        }

        public static IAuthentication Membership
        {
            get { return Kernel.Get<IAuthentication>(); }
        }
        public static Context CurrentContext { get; set; }

        static CapiApplication()
        {
            Kernel = new StandardKernel(new AndroidCoreRegistry("connectString", false));
           
        }

     /*   public CapiApplication()
		{
		}*/

        protected CapiApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            CurrentContext = this;

            Kernel.Bind<Context>().ToConstant(this.ApplicationContext);

            //  Kernel.Bind<IAuthentication>().ToConstant(new AndroidAuthentication());
            NcqrsInit.Init(Kernel);
            var eventStore = new SQLiteEventStore(this.ApplicationContext);
            eventStore.ClearDB();
            NcqrsEnvironment.SetDefault(eventStore);


            #region register handlers

            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;
            ProjectionStorage.ClearStorage();
            var eventHandler =
                new CompleteQuestionnaireViewDenormalizer(
                    Kernel.Get<IDenormalizerStorage<CompleteQuestionnaireView>>(), ProjectionStorage);
            bus.RegisterHandler(eventHandler, typeof (SnapshootLoaded));
            bus.RegisterHandler(eventHandler, typeof (AnswerSet));
            bus.RegisterHandler(eventHandler, typeof (CommentSet));
            bus.RegisterHandler(eventHandler, typeof (ConditionalStatusChanged));
            bus.RegisterHandler(eventHandler, typeof (PropagatableGroupAdded));
            bus.RegisterHandler(eventHandler, typeof (PropagatableGroupDeleted));
            bus.RegisterHandler(eventHandler, typeof (QuestionnaireStatusChanged));

            var dashboardeventHandler =
                new DashboardDenormalizer(Kernel.Get<IDenormalizerStorage<DashboardModel>>());
            bus.RegisterHandler(dashboardeventHandler, typeof (SnapshootLoaded));
            bus.RegisterHandler(dashboardeventHandler, typeof (QuestionnaireStatusChanged));


            var usereventHandler =
                new UserDenormalizer(Kernel.Get<IDenormalizerStorage<UserView>>());
            bus.RegisterHandler(usereventHandler, typeof (NewUserCreated));

            #endregion

            GenerateEvents(bus);
        }

        protected void GenerateEvents(InProcessEventBus bus)
        {

            var stream = new UncommittedEventStream(Guid.NewGuid());
            //  var payload = new NewCompleteQuestionnaireCreated();

            #region init




            CompleteQuestionnaireDocument root = DesserializeEmbededResource<CompleteQuestionnaireDocument>("initEvent.txt");
            CompleteQuestionnaireDocument researchQ = DesserializeEmbededResource<CompleteQuestionnaireDocument>("researchDeptSurvey.txt");
            NewUserCreated userEvent = DesserializeEmbededResource<NewUserCreated>("userEvent.txt");
            #endregion

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
            stream.Append(userEventUcmt);
            stream.Append(eventTempl);
            stream.Append(rEventTempl);
            
            Kernel.Get<IEventStore>().Store(stream);
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