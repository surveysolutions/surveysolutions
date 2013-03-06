using System;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Runtime;

using AndroidApp.Core.Model.Authorization;
using AndroidApp.Core.Model.EventHandlers;
using AndroidApp.Core.Model.FileStorage;
using AndroidApp.Core.Model.ViewModel.Dashboard;
using AndroidApp.Core.Model.ViewModel.QuestionnaireDetails;
using AndroidApp.Core.Unmanaged;
using AndroidApp.Injections;
using AndroidNcqrs.Eventing.Storage.SQLite;
using Cirrious.MvvmCross.Droid.Platform;
using Main.Core;
using Main.Core.Documents;
using Main.Core.EventHandlers;
using Main.Core.Events.File;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.Events.User;
using Main.Core.Services;
using Main.Core.View;
using Main.Core.View.User;
using Main.DenormalizerStorage;
using Mono.Android.Crasher;
using Mono.Android.Crasher.Attributes;
using Mono.Android.Crasher.Data.Submit;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;

using Ninject;
using UserDenormalizer = AndroidApp.Core.Model.EventHandlers.UserDenormalizer;

namespace AndroidApp
{
    using Main.Synchronization.SycProcessRepository;

    [Application]
    [Crasher(UseCustomData = false)]
    public class CapiApplication : Application
    {
        #region static properties

        public static TOutput LoadView<TInput, TOutput>(TInput input)
        {
            return Kernel.Get<IViewRepository>().Load<TInput, TOutput>(input);
        }

        public static ICommandService CommandService
        {
            get { return NcqrsEnvironment.Get<ICommandService>(); }
        }

        public static IAuthentication Membership
        {
            get { return Kernel.Get<IAuthentication>(); }
        }
        public static IFileStorageService FileStorageService
        {
            get { return Kernel.Get<IFileStorageService>(); }
        }
        public static IKernel Kernel
        {
            get
            {
                if (Context == null)
                    return null;
                var capiApp = Context.ApplicationContext as CapiApplication;
                if (capiApp == null)
                    return null;
                return capiApp.kernel;
            }
        }

        #endregion


        protected CapiApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            kernel = new StandardKernel(new AndroidCoreRegistry("connectString", false));
            kernel.Bind<Context>().ToConstant(this);
            kernel.Bind<ISyncProcessRepository>().To<SyncProcessRepository>();
            NcqrsInit.Init(kernel);
     
            NcqrsEnvironment.SetDefault<IStreamableEventStore>(NcqrsEnvironment.Get<IEventStore>() as SQLiteEventStore);

            #region register handlers

            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;
            var eventHandler =
                new CompleteQuestionnaireViewDenormalizer(
                    kernel.Get<IDenormalizerStorage<CompleteQuestionnaireView>>());
            bus.RegisterHandler(eventHandler, typeof(SnapshootLoaded));
            bus.RegisterHandler(eventHandler, typeof(AnswerSet));
            bus.RegisterHandler(eventHandler, typeof(CommentSet));
            bus.RegisterHandler(eventHandler, typeof(ConditionalStatusChanged));
            bus.RegisterHandler(eventHandler, typeof(PropagatableGroupAdded));
            bus.RegisterHandler(eventHandler, typeof(PropagatableGroupDeleted));
            bus.RegisterHandler(eventHandler, typeof(QuestionnaireStatusChanged));

            var dashboardeventHandler =
                new DashboardDenormalizer(kernel.Get<IDenormalizerStorage<DashboardModel>>());
            bus.RegisterHandler(dashboardeventHandler, typeof(SnapshootLoaded));
            bus.RegisterHandler(dashboardeventHandler, typeof(QuestionnaireStatusChanged));


            var usereventHandler =
                new UserDenormalizer(kernel.Get<IDenormalizerStorage<UserView>>());
            bus.RegisterHandler(usereventHandler, typeof(NewUserCreated));

            var fileSorage = new FileStoreDenormalizer(kernel.Get<IDenormalizerStorage<FileDescription>>(),
                                                       new FileStorageService());
            bus.RegisterHandler(fileSorage, typeof(FileUploaded));
            bus.RegisterHandler(fileSorage, typeof(FileDeleted));
            #endregion


        }
        public override void OnCreate()
        {
            Java.Lang.Thread.DefaultUncaughtExceptionHandler = new
                CMUncaughtExceptionHandler();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            base.OnCreate();
            CrashManager.Initialize(this);
            CrashManager.AttachSender(() => new FileReportSender("CAPI"));
            var manager = this.GetSystemService(Context.ActivityService) as ActivityManager;
            var topActivity = manager.GetRunningTasks(1).Last().TopActivity;
            if (!topActivity.ClassName.Contains(typeof(SplashScreenActivity).Name))
                GenerateEvents();
        }
        
        private readonly IKernel kernel;
        
        public  static void Restart()
        {
            Intent i = Context.PackageManager.GetLaunchIntentForPackage(Context.PackageName);
            i.AddFlags(ActivityFlags.ClearTop);
            Context.StartActivity(i);
            
        }
        
        public static void GenerateEvents()
        {
            var _setup = MvxAndroidSetupSingleton.GetOrCreateSetup(CapiApplication.Context);

                // initialize app if necessary
                if (_setup.State == Cirrious.MvvmCross.Platform.MvxBaseSetup.MvxSetupState.Uninitialized)
                {
                    _setup.Initialize();
                }
            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;
            var eventStore = NcqrsEnvironment.Get<IEventStore>() as SQLiteEventStore;
          
            var events = eventStore.GetAllEvents();
            foreach (CommittedEvent committedEvent in events)
            {
                bus.Publish(committedEvent);
            }
      /*      eventStore.ClearDB();
               var stream = new UncommittedEventStream(Guid.NewGuid());
               //  var payload = new NewCompleteQuestionnaireCreated();

               #region init

               CompleteQuestionnaireDocument root = DesserializeEmbededResource<CompleteQuestionnaireDocument>("initEvent.txt");
               CompleteQuestionnaireDocument researchQ = DesserializeEmbededResource<CompleteQuestionnaireDocument>("researchDeptSurvey.txt");
               NewUserCreated userEvent = DesserializeEmbededResource<NewUserCreated>("userEvent.txt");

               for (int i = 0; i < 10; i++)
               {

                   root.PublicKey = Guid.NewGuid();

                   var eventTempl = new UncommittedEvent(Guid.NewGuid(),
                                                         root.PublicKey, 1, 0, DateTime.Now,
                                                         new SnapshootLoaded()
                                                         {
                                                             Template = new Snapshot(root.PublicKey, 1, root)
                                                         }, new Version());
                   stream.Append(eventTempl);
                   bus.Publish(eventTempl);
               }
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

               stream.Append(rEventTempl);

               eventStore.Store(stream);
               bus.Publish(userEventUcmt);

               bus.Publish(rEventTempl);*/

        }
   /*     protected static T DesserializeEmbededResource<T>(string fileName)
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
        }*/

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
          
            
        }

   
    }
}