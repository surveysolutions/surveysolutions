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
using Main.Core.View;
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
                new CompleteQuestionnaireViewDenormalizer(Kernel.Get<IDenormalizerStorage<CompleteQuestionnaireView>>(), ProjectionStorage);
            bus.RegisterHandler(eventHandler, typeof(SnapshootLoaded));
            bus.RegisterHandler(eventHandler, typeof(AnswerSet));
            bus.RegisterHandler(eventHandler, typeof(CommentSet));
            bus.RegisterHandler(eventHandler, typeof(ConditionalStatusChanged));
            bus.RegisterHandler(eventHandler, typeof(PropagatableGroupAdded));
            bus.RegisterHandler(eventHandler, typeof(PropagatableGroupDeleted));


            var dashboardeventHandler =
               new DashboardDenormalizer(Kernel.Get<IDenormalizerStorage<DashboardModel>>());
            bus.RegisterHandler(dashboardeventHandler, typeof(SnapshootLoaded));
            #endregion

            GenerateEvents(bus);
		}
        protected void GenerateEvents(InProcessEventBus bus)
        {

            var stream = new UncommittedEventStream(Guid.NewGuid());
            //  var payload = new NewCompleteQuestionnaireCreated();

            #region init

            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            //var data = Encoding.Default.GetString("");
            string s = string.Empty;
            using (Stream streamEmbededRes = Assembly.GetExecutingAssembly()
                               .GetManifestResourceStream("AndroidApp." + "initEvent.txt"))
            using (StreamReader reader = new StreamReader(streamEmbededRes))
            {
                s = reader.ReadToEnd();
            }



            CompleteQuestionnaireDocument root = JsonConvert.DeserializeObject<CompleteQuestionnaireDocument>(s, settings);
            #endregion

            var eventTempl = new UncommittedEvent(Guid.Parse("05c9a227-d249-41d8-9a11-99e60e0a9eda"),
                                               Guid.Parse("488b95d8-0783-40f5-a9e0-732f5ea44286"), 1, 0, DateTime.Now,
                                               new SnapshootLoaded()
                                                   {
                                                       Template = new Snapshot(root.PublicKey, 1, root)
                                                   }, new Version());
            stream.Append(eventTempl);
            Kernel.Get<IEventStore>().Store(stream);
            bus.Publish(eventTempl);

        }
    }
}