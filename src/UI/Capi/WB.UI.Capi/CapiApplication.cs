using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.FileStorage;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Login;
using Cirrious.MvvmCross.Droid.Platform;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Events.File;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
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
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Logging.AndroidLogger;
using WB.Core.Infrastructure.InformationSupplier;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.EventHandler;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.ExpressionProcessor;
using WB.UI.Capi.Extensions;
using WB.UI.Capi.Injections;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Extensions;

namespace WB.UI.Capi
{
#if DEBUG 
    [Application(Debuggable=true)] 
#else
    [Application(Debuggable = false)]
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

        public static IDataCollectionAuthentication Membership
        {
            get { return Kernel.Get<IDataCollectionAuthentication>(); }
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
                    this.kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>(), this.kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>(),
                    this.kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>>());

            bus.RegisterHandler(eventHandler, typeof (InterviewSynchronized));
            bus.RegisterHandler(eventHandler, typeof (MultipleOptionsQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (NumericIntegerQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (NumericRealQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (NumericQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (TextQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (TextListQuestionAnswered));
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
            bus.RegisterHandler(eventHandler, typeof(RosterRowAdded));
            bus.RegisterHandler(eventHandler, typeof(RosterRowRemoved));
            bus.RegisterHandler(eventHandler, typeof(SynchronizationMetadataApplied));
            bus.RegisterHandler(eventHandler, typeof(GeoLocationQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(AnswerRemoved));
            bus.RegisterHandler(eventHandler, typeof(SingleOptionLinkedQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(MultipleOptionsLinkedQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(RosterRowTitleChanged));

            bus.RegisterHandler(eventHandler, typeof(TextListQuestionAnswered));


            var answerOptionsForLinkedQuestionsDenormalizer = this.kernel.Get<AnswerOptionsForLinkedQuestionsDenormalizer>();

            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(AnswerRemoved));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(TextQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericIntegerQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericRealQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(DateTimeQuestionAnswered));
        }

        private void InitTemplateStorage(InProcessEventBus bus)
        {
            var templateDenoramalizer = new QuestionnaireDenormalizer(this.kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>());
            bus.RegisterHandler(templateDenoramalizer, typeof(TemplateImported));
            
            var rosterStructureDenormalizer =
                new QuestionnaireRosterStructureDenormalizer(
                    this.kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>>());

            bus.RegisterHandler(rosterStructureDenormalizer, typeof(TemplateImported));
        }

        private void InitFileStorage(InProcessEventBus bus)
        {
            var fileSorage = new AndroidFileStoreDenormalizer(this.kernel.Get<IReadSideRepositoryWriter<FileDescription>>());
            bus.RegisterHandler(fileSorage, typeof (FileUploaded));
            bus.RegisterHandler(fileSorage, typeof (FileDeleted));
        }

        private void InitUserStorage(InProcessEventBus bus)
        {
            var usereventHandler =
                new UserDenormalizer(this.kernel.Get<IReadSideRepositoryWriter<LoginDTO>>());
            bus.RegisterHandler(usereventHandler, typeof (NewUserCreated));
            bus.RegisterHandler(usereventHandler, typeof(UserChanged));
        }

        private void InitDashboard(InProcessEventBus bus)
        {
            var dashboardeventHandler =
                new DashboardDenormalizer(this.kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>(),
                                          this.kernel.Get<IReadSideRepositoryWriter<SurveyDto>>(),
                                          this.kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>());

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

             this.RestoreAppState();

             // initialize app if necessary
            MvxAndroidSetupSingleton.EnsureSingletonAvailable(this);
            MvxAndroidSetupSingleton.Instance.EnsureInitialized();

            this.kernel = new StandardKernel(
                new CapiBoundedContextModule(),
                new AndroidCoreRegistry(),
                new AndroidModelModule(),
                new AndroidLoggingModule(),
                new DataCollectionSharedKernelModule(),
                new ExpressionProcessorModule());

            CrashManager.Initialize(this);
            CrashManager.AttachSender(() => new FileReportSender("CAPI", this.kernel.Get<IInfoFileSupplierRegistry>()));
         
            this.kernel.Bind<Context>().ToConstant(this);
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.kernel));
            this.kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);

            NcqrsInit.Init(this.kernel);
       
            NcqrsEnvironment.SetDefault<ISnapshotStore>(Kernel.Get<ISnapshotStore>());
            NcqrsEnvironment.SetDefault(NcqrsEnvironment.Get<IEventStore>() as IStreamableEventStore);
            var domainrepository = new DomainRepository(NcqrsEnvironment.Get<IAggregateRootCreationStrategy>(), NcqrsEnvironment.Get<IAggregateSnapshotter>());
            this.kernel.Bind<IDomainRepository>().ToConstant(domainrepository);
            this.kernel.Bind<ICommandService>().ToConstant(CommandService);

            this.kernel.Unbind<IAnswerOnQuestionCommandService>();
            this.kernel.Bind<IAnswerOnQuestionCommandService>().To<AnswerOnQuestionCommandService>().InSingletonScope();
            this.kernel.Bind<IQuestionViewFactory>().To<DefaultQuestionViewFactory>();
            
            #region register handlers

            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;

            this.InitTemplateStorage(bus);

            this.InitInterviewStorage(bus);

            this.InitUserStorage(bus);

            this.InitFileStorage(bus);

            this.InitDashboard(bus);
            
            #endregion
        }

        private void RestoreAppState()
        {
            AndroidEnvironment.UnhandledExceptionRaiser += this.AndroidEnvironmentUnhandledExceptionRaiser;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                AndroidEnvironment.UnhandledExceptionRaiser -= this.AndroidEnvironmentUnhandledExceptionRaiser;
            }

            base.Dispose(disposing);
        }
        private void AndroidEnvironmentUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            this.ClearAllBackStack<SplashScreen>();

            var questionnarieDenormalizer =
                this.kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>() as
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