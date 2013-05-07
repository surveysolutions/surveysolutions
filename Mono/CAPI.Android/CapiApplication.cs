using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
using AndroidNcqrs.Eventing.Storage.SQLite;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using CAPI.Android.Core.Model.Authorization;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.ProjectionStorage;
using CAPI.Android.Core.Model.FileStorage;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Unmanaged;
using CAPI.Android.Extensions;
using CAPI.Android.Injections;
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
using Ncqrs.Restoring.EventStapshoot.EventStores;
using Ninject;
using Main.Synchronization.SycProcessRepository;
using UserDenormalizer = CAPI.Android.Core.Model.EventHandlers.UserDenormalizer;

namespace CAPI.Android
{
    #if RELEASE 
    [Application(Debuggable=false)] 
    #else
    [Application(Debuggable = true)]
    #endif
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
            var _setup = MvxAndroidSetupSingleton.GetOrCreateSetup(Context);

            // initialize app if necessary
            if (_setup.State == Cirrious.MvvmCross.Platform.MvxBaseSetup.MvxSetupState.Uninitialized)
            {
                _setup.Initialize();
            }

            kernel = new StandardKernel(new AndroidCoreRegistry("connectString", false));
            kernel.Bind<Context>().ToConstant(this);
            kernel.Bind<ISyncProcessRepository>().To<SyncProcessRepository>();
            NcqrsInit.Init(kernel);
     
            NcqrsEnvironment.SetDefault<IStreamableEventStore>(NcqrsEnvironment.Get<IEventStore>() as IStreamableEventStore);

            #region register handlers

            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;

            InitQuestionnariesStorage(bus);

            InitUserStorage(bus);

            InitFileStorage(bus);

            InitDashboard(bus);

            #endregion

           
        }

        private void InitQuestionnariesStorage(InProcessEventBus bus)
        {
            var eventHandler =
                new CompleteQuestionnaireViewDenormalizer(
                    kernel.Get<IDenormalizerStorage<CompleteQuestionnaireView>>());
            bus.RegisterHandler(eventHandler, typeof (SnapshootLoaded));
            bus.RegisterHandler(eventHandler, typeof (AnswerSet));
            bus.RegisterHandler(eventHandler, typeof (CommentSet));
            bus.RegisterHandler(eventHandler, typeof (ConditionalStatusChanged));
            bus.RegisterHandler(eventHandler, typeof (PropagatableGroupAdded));
            bus.RegisterHandler(eventHandler, typeof (PropagatableGroupDeleted));
            bus.RegisterHandler(eventHandler, typeof (QuestionnaireStatusChanged));
        }

        private void InitFileStorage(InProcessEventBus bus)
        {
            var fileSorage = new FileStoreDenormalizer(kernel.Get<IDenormalizerStorage<FileDescription>>(),
                                                       new FileStorageService());
            bus.RegisterHandler(fileSorage, typeof (FileUploaded));
            bus.RegisterHandler(fileSorage, typeof (FileDeleted));
        }

        private void InitUserStorage(InProcessEventBus bus)
        {
            var mvvmSqlLiteUserStorage = new SqliteDenormalizerStorage<LoginDTO>();
            var membership = new AndroidAuthentication(mvvmSqlLiteUserStorage);
            kernel.Bind<IAuthentication>().ToConstant(membership);
            var usereventHandler =
                new UserDenormalizer(mvvmSqlLiteUserStorage);
            bus.RegisterHandler(usereventHandler, typeof (NewUserCreated));
        }

        private void InitDashboard(InProcessEventBus bus)
        {
            var surveyStore = new SqliteDenormalizerStorage<SurveyDto>();
            var questionnaireStore = new SqliteDenormalizerStorage<QuestionnaireDTO>();
            
            Kernel.Unbind<IDenormalizerStorage<SurveyDto>>();
            Kernel.Bind<IDenormalizerStorage<SurveyDto>>().ToConstant(surveyStore);

            Kernel.Unbind<IDenormalizerStorage<QuestionnaireDTO>>();
            Kernel.Bind<IDenormalizerStorage<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            
            var dashboardeventHandler =
                new DashboardDenormalizer(questionnaireStore, surveyStore);
            bus.RegisterHandler(dashboardeventHandler, typeof (SnapshootLoaded));
            bus.RegisterHandler(dashboardeventHandler, typeof (QuestionnaireStatusChanged));
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
        
        private IKernel kernel;
        public  static void Restart()
        {
            Intent i = Context.PackageManager.GetLaunchIntentForPackage(Context.PackageName);
            i.AddFlags(ActivityFlags.ClearTop);
            Context.StartActivity(i);
            
        }
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