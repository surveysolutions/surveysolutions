using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.SyncCacher;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Extensions;
using CAPI.Android.Injections;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.ViewModels;
using CommonServiceLocator.NinjectAdapter;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Events.File;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.Events.User;
using Main.Core.Services;
using Main.Core.View;
using Main.DenormalizerStorage;
using Microsoft.Practices.ServiceLocation;
using Mono.Android.Crasher;
using Mono.Android.Crasher.Attributes;
using Mono.Android.Crasher.Data.Submit;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ninject;
using WB.Core.GenericSubdomains.Logging.AndroidLogger;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
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
            var factory = Kernel.TryGet<IViewFactory<TInput, TOutput>>();

            return factory == null ? default(TOutput) : factory.Load(input);
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

        /*public static ISyncCacher SyncCacher
        {
            get { return Kernel.Get<ISyncCacher>(); }
        }*/

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
           
           

        }

        private void InitQuestionnariesStorage(InProcessEventBus bus)
        {
            var eventHandler =
                new CompleteQuestionnaireViewDenormalizer(
                    kernel.Get<IReadSideRepositoryWriter<CompleteQuestionnaireView>>(), kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>());

            bus.RegisterHandler(eventHandler, typeof (InterviewSynchronized));
            bus.RegisterHandler(eventHandler, typeof (MultipleOptionsQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (NumericQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (TextQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (SingleOptionQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (DateTimeQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (GroupDisabled));
            bus.RegisterHandler(eventHandler, typeof (GroupEnabled));
            bus.RegisterHandler(eventHandler, typeof (QuestionDisabled));
            bus.RegisterHandler(eventHandler, typeof (QuestionEnabled));
            bus.RegisterHandler(eventHandler, typeof (AnswerDeclaredInvalid));
            bus.RegisterHandler(eventHandler, typeof (AnswerDeclaredValid));
            bus.RegisterHandler(eventHandler, typeof(AnswerCommented));
            bus.RegisterHandler(eventHandler, typeof(InterviewCompleted));
            bus.RegisterHandler(eventHandler, typeof(InterviewRestarted));
            bus.RegisterHandler(eventHandler, typeof(GroupPropagated));
            bus.RegisterHandler(eventHandler, typeof(InterviewMetaInfoUpdated));
            bus.RegisterHandler(eventHandler, typeof(GeoLocationQuestionAnswered));
           
        }

        private void InitTemplateStorage(InProcessEventBus bus)
        {
            var fileSorage = new QuestionnaireDenormalizer(kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>());
            bus.RegisterHandler(fileSorage, typeof(TemplateImported));
        }

        private void InitFileStorage(InProcessEventBus bus)
        {
            var fileSorage = new AndroidFileStoreDenormalizer(kernel.Get<IReadSideRepositoryWriter<FileDescription>>(),
                                                       kernel.Get<IFileStorageService>());
            bus.RegisterHandler(fileSorage, typeof (FileUploaded));
            bus.RegisterHandler(fileSorage, typeof (FileDeleted));
        }

        private void InitUserStorage(InProcessEventBus bus)
        {
            var usereventHandler =
                new UserDenormalizer(kernel.Get<IReadSideRepositoryWriter<LoginDTO>>());
            bus.RegisterHandler(usereventHandler, typeof (NewUserCreated));
        }

        private void InitDashboard(InProcessEventBus bus)
        {
            var dashboardeventHandler =
                new DashboardDenormalizer(kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>(),
                                          kernel.Get<IReadSideRepositoryWriter<SurveyDto>>(),
                                          kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>());
           
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewMetaInfoUpdated));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewRestarted));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewCompleted));
            bus.RegisterHandler(dashboardeventHandler, typeof(TemplateImported));
            
        }

        private void InitChangeLog(InProcessEventBus bus)
        {
           
            var changeLogHandler = new CommitDenormalizer(Kernel.Get<IChangeLogManipulator>());
            bus.RegisterHandler(changeLogHandler, typeof(InterviewDeclaredInvalid));
            bus.RegisterHandler(changeLogHandler, typeof(InterviewDeclaredValid));
            bus.RegisterHandler(changeLogHandler, typeof(InterviewRestarted));
            bus.RegisterHandler(changeLogHandler, typeof(InterviewSynchronized));
        }

        public override void OnCreate()
        {
            base.OnCreate();

            CrashManager.Initialize(this);
            CrashManager.AttachSender(() => new FileReportSender("CAPI"));
            RestoreAppState();

             // initialize app if necessary
            MvxAndroidSetupSingleton.EnsureSingletonAvailable(this);
            MvxAndroidSetupSingleton.Instance.EnsureInitialized();

            kernel = new StandardKernel(
                new AndroidCoreRegistry(),
                new AndroidModelModule(),
                new AndroidLoggingModule(),
                new DataCollectionSharedKernelModule());

            kernel.Bind<Context>().ToConstant(this);
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.kernel));
            this.kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);
            NcqrsInit.Init(kernel);
            NcqrsEnvironment.SetDefault<ISnapshotStore>(Kernel.Get<ISnapshotStore>());
            NcqrsEnvironment.SetDefault(NcqrsEnvironment.Get<IEventStore>() as IStreamableEventStore);
            var domainrepository = new DomainRepository(NcqrsEnvironment.Get<IAggregateRootCreationStrategy>(), NcqrsEnvironment.Get<IAggregateSnapshotter>());
            kernel.Bind<IDomainRepository>().ToConstant(domainrepository);
            #region register handlers

            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;

            InitTemplateStorage(bus);

            InitQuestionnariesStorage(bus);

            InitUserStorage(bus);

            InitFileStorage(bus);

            InitDashboard(bus);

            InitChangeLog(bus);

            #endregion
        }

        private void RestoreAppState()
        {
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentUnhandledExceptionRaiser;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            AndroidEnvironment.UnhandledExceptionRaiser -= AndroidEnvironmentUnhandledExceptionRaiser;
        }
        private void AndroidEnvironmentUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            this.ClearAllBackStack<SplashScreen>();

            var questionnarieDenormalizer =
                kernel.Get<IReadSideRepositoryWriter<CompleteQuestionnaireView>>() as
                InMemoryReadSideRepositoryAccessor<CompleteQuestionnaireView>;
            if (questionnarieDenormalizer != null)
                questionnarieDenormalizer.Clear();
        }

        private IKernel kernel;

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }
    }
}