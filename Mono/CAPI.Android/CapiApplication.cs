using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using CAPI.Android.Controls.QuestionnaireDetails.ScreenItems;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Extensions;
using CAPI.Android.Injections;
using Cirrious.MvvmCross.Droid.Platform;
using CommonServiceLocator.NinjectAdapter;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Events.File;
using Main.Core.Events.Questionnaire;
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
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Logging.AndroidLogger;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.EventHandler;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.ExpressionProcessor;
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

        private void InitInterviewStorage(InProcessEventBus bus)
        {
            var eventHandler =
                new InterviewViewModelDenormalizer(
                    kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>(), kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>(),
                    kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure>>());

            bus.RegisterHandler(eventHandler, typeof (InterviewSynchronized));
            bus.RegisterHandler(eventHandler, typeof (MultipleOptionsQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (NumericIntegerQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (NumericRealQuestionAnswered));
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
            bus.RegisterHandler(eventHandler, typeof(SynchronizationMetadataApplied));
            bus.RegisterHandler(eventHandler, typeof(GeoLocationQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(AnswerRemoved));
            bus.RegisterHandler(eventHandler, typeof(SingleOptionLinkedQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(MultipleOptionsLinkedQuestionAnswered));


            var answerOptionsForLinkedQuestionsDenormalizer = kernel.Get<AnswerOptionsForLinkedQuestionsDenormalizer>();

            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(AnswerRemoved));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(TextQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericIntegerQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericRealQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(DateTimeQuestionAnswered));
        }

        private void InitTemplateStorage(InProcessEventBus bus)
        {
            var templateDenoramalizer = new QuestionnaireDenormalizer(kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>());
            bus.RegisterHandler(templateDenoramalizer, typeof(TemplateImported));
            
            var propagationStructureDenormalizer =
                new QuestionnairePropagationStructureDenormalizer(
                    kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnairePropagationStructure>>());

            bus.RegisterHandler(propagationStructureDenormalizer, typeof(TemplateImported));
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
            bus.RegisterHandler(usereventHandler, typeof(UserChanged));
        }

        private void InitDashboard(InProcessEventBus bus)
        {
            var dashboardeventHandler =
                new DashboardDenormalizer(kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>(),
                                          kernel.Get<IReadSideRepositoryWriter<SurveyDto>>(),
                                          kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>());

            bus.RegisterHandler(dashboardeventHandler, typeof(SynchronizationMetadataApplied));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewDeclaredValid));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewDeclaredInvalid));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewStatusChanged));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewSynchronized));
            bus.RegisterHandler(dashboardeventHandler, typeof(TemplateImported));
            
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
                new DataCollectionSharedKernelModule(),
                new ExpressionProcessorModule());

            kernel.Bind<Context>().ToConstant(this);
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.kernel));
            this.kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);

            NcqrsInit.Init(kernel);
       
            NcqrsEnvironment.SetDefault<ISnapshotStore>(Kernel.Get<ISnapshotStore>());
            NcqrsEnvironment.SetDefault(NcqrsEnvironment.Get<IEventStore>() as IStreamableEventStore);
            var domainrepository = new DomainRepository(NcqrsEnvironment.Get<IAggregateRootCreationStrategy>(), NcqrsEnvironment.Get<IAggregateSnapshotter>());
            kernel.Bind<IDomainRepository>().ToConstant(domainrepository);
            Kernel.Unbind<IAnswerOnQuestionCommandService>();
            Kernel.Bind<IAnswerOnQuestionCommandService>()
                  .ToConstant(new AnswerOnQuestionCommandService(CommandService));
            #region register handlers

            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;

            InitTemplateStorage(bus);

            this.InitInterviewStorage(bus);

            InitUserStorage(bus);

            InitFileStorage(bus);

            InitDashboard(bus);
            
            #endregion
        }

        private void RestoreAppState()
        {
            AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentUnhandledExceptionRaiser;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                AndroidEnvironment.UnhandledExceptionRaiser -= AndroidEnvironmentUnhandledExceptionRaiser;
            }

            base.Dispose(disposing);
        }
        private void AndroidEnvironmentUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            this.ClearAllBackStack<SplashScreen>();

            var questionnarieDenormalizer =
                kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>() as
                InMemoryReadSideRepositoryAccessor<InterviewViewModel>;
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