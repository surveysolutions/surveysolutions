using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
using AndroidNcqrs.Eventing.Storage.SQLite;
using CAPI.Android.Core.Model.Authorization;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.ProjectionStorage;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Unmanaged;
using CAPI.Android.Extensions;
using CAPI.Android.Injections;
using Cirrious.MvvmCross.Droid.Platform;
using Main.Core;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.Events.User;
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
using Ncqrs.Eventing.Storage;
using Ncqrs.Restoring.EventStapshoot;
using Ncqrs.Restoring.EventStapshoot.EventStores;
using Ninject;
using Main.Synchronization.SycProcessRepository;

namespace CAPI.Android
{
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

        static IList<Guid> DashboardsRestored
        {
            get
            {
                if (Context == null)
                    return null;
                var capiApp = Context.ApplicationContext as CapiApplication;
                if (capiApp == null)
                    return null;
                return capiApp.dashboardsRestored;
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

         


            var usereventHandler =
                new UserDenormalizer(kernel.Get<IProjectionStorage>(), kernel.Get<IDenormalizerStorage<UserView>>());
            bus.RegisterHandler(usereventHandler, typeof(NewUserCreated));

            var dashboardeventHandler =
             new DashboardDenormalizer( kernel.Get<IDenormalizerStorage<DashboardModel>>());
            bus.RegisterHandler(dashboardeventHandler, typeof(SnapshootLoaded));
            bus.RegisterHandler(dashboardeventHandler, typeof(QuestionnaireStatusChanged));
            bus.RegisterHandler(dashboardeventHandler, typeof(QuestionnaireAssignmentChanged));
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
            if (!topActivity.ClassName.Contains(typeof (SplashScreenActivity).Name))
                this.ClearAllBackStack<SplashScreenActivity>();
        }
        
        private readonly IKernel kernel;
        private readonly IList<Guid> dashboardsRestored = new List<Guid>();
        public  static void Restart()
        {
            Intent i = Context.PackageManager.GetLaunchIntentForPackage(Context.PackageName);
            i.AddFlags(ActivityFlags.ClearTop);
            Context.StartActivity(i);
            
        }
        public static  void SaveProjections()
        {
            var persistanceStorage = CapiApplication.Kernel.Get<IProjectionStorage>();
            persistanceStorage.SaveOrUpdateProjection(
                CapiApplication.Kernel.Get<IDenormalizerStorage<UserView>>().Query().ToList(), Guid.Empty);
            var dashboards = CapiApplication.Kernel.Get<IDenormalizerStorage<DashboardModel>>().Query().ToList();

            foreach (DashboardModel dashboardModel in dashboards)
            {
                DashboardsRestored.Add(dashboardModel.OwnerKey);
                var roots = new List<Guid>();
                foreach (DashboardSurveyItem dashboardSurveyItem in dashboardModel.Surveys)
                {
                    roots.AddRange(dashboardSurveyItem.ActiveItems.Select(d => d.PublicKey));
                }
                persistanceStorage.SaveOrUpdateProjection(roots, dashboardModel.OwnerKey);
            }
        }
        public static void GenerateEvents(Guid userKey)
        {
            if(DashboardsRestored.Contains(userKey))
                return;
            
            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;
            var eventStore = NcqrsEnvironment.Get<IEventStore>() as ISnapshootEventStore;
            var persistanceStorage = CapiApplication.Kernel.Get<IProjectionStorage>();
            var roots = persistanceStorage.RestoreProjection<List<Guid>>(userKey) ?? new List<Guid>();
            foreach (Guid root in roots)
            {
                long minVersion = 0;
                var snapshot = eventStore.GetLatestSnapshoot(root);
                if (snapshot != null)
                {
                    bus.Publish(snapshot);
                    minVersion = snapshot.EventSequence + 1;
                }
                foreach (CommittedEvent committedEvent in
                        eventStore.ReadFrom(root, minVersion, long.MaxValue))
                {
                    bus.Publish(committedEvent);
                }
            }
            DashboardsRestored.Add(userKey);
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



        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }
    }
}