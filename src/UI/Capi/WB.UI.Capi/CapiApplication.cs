using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.Content;
using Android.Runtime;
using CAPI.Android.Core.Model.ViewModel.Login;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Droid.Platform;
using Main.Core.Events.File;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Main.DenormalizerStorage;
using Microsoft.Practices.ServiceLocation;
using Mono.Android.Crasher;
using Mono.Android.Crasher.Attributes;
using Mono.Android.Crasher.Data.Submit;
using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.ErrorReporting;
using WB.Core.GenericSubdomains.Logging.AndroidLogger;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement;
using WB.UI.Capi.EventHandlers;
using WB.UI.Capi.FileStorage;
using WB.UI.Capi.Implementations.Navigation;
using WB.UI.Capi.Implementations.Services;
using WB.UI.Capi.Injections;
using WB.UI.Capi.Settings;
using WB.UI.Capi.Syncronization.Implementation;
using WB.UI.Capi.ViewModel.Dashboard;
using WB.UI.Shared.Android;
using WB.UI.Shared.Android.Controls.ScreenItems;
using WB.UI.Shared.Android.Extensions;
using IInfoFileSupplierRegistry = WB.Core.GenericSubdomains.Utils.Services.IInfoFileSupplierRegistry;

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
        public class ServiceLocationModule : NinjectModule
        {
            public override void Load()
            {
                ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.Kernel));
                this.Kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);
            }
        }

        #region static properties

        public static TOutput LoadView<TInput, TOutput>(TInput input)
        {
            var factory = Kernel.TryGet<IViewFactory<TInput, TOutput>>();

            return factory == null ? default(TOutput) : factory.Load(input);
        }

        public static ICommandService CommandService
        {
            get { return Kernel.Get<ICommandService>(); }
        }

        public static IDataCollectionAuthentication Membership
        {
            get { return Kernel.Get<IDataCollectionAuthentication>(); }
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
        }

        private void RegisterInterviewHandlerInBus(InProcessEventBus bus, InterviewViewModelDenormalizer eventHandler, 
            AnswerOptionsForLinkedQuestionsDenormalizer answerOptionsForLinkedQuestionsDenormalizer,
            AnswerOptionsForCascadingQuestionsDenormalizer answerOptionsForCascadingQuestionsDenormalizer)
        {
            
            bus.RegisterHandler(eventHandler, typeof (InterviewSynchronized));
            bus.RegisterHandler(eventHandler, typeof (MultipleOptionsQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (NumericIntegerQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (NumericRealQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (TextQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (TextListQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (SingleOptionQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (DateTimeQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof (GroupsDisabled));
            bus.RegisterHandler(eventHandler, typeof (GroupsEnabled));
            bus.RegisterHandler(eventHandler, typeof (QuestionsDisabled));
            bus.RegisterHandler(eventHandler, typeof (QuestionsEnabled));
            bus.RegisterHandler(eventHandler, typeof (AnswersDeclaredInvalid));
            bus.RegisterHandler(eventHandler, typeof (AnswersDeclaredValid));
            bus.RegisterHandler(eventHandler, typeof(AnswerCommented));
            bus.RegisterHandler(eventHandler, typeof(InterviewCompleted));
            bus.RegisterHandler(eventHandler, typeof(InterviewRestarted));
            bus.RegisterHandler(eventHandler, typeof(GroupPropagated));
            bus.RegisterHandler(eventHandler, typeof(RosterInstancesAdded));
            bus.RegisterHandler(eventHandler, typeof(RosterInstancesRemoved));
            bus.RegisterHandler(eventHandler, typeof(SynchronizationMetadataApplied));
            bus.RegisterHandler(eventHandler, typeof(GeoLocationQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(AnswersRemoved));
            bus.RegisterHandler(eventHandler, typeof(SingleOptionLinkedQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(MultipleOptionsLinkedQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(RosterInstancesTitleChanged));
            bus.RegisterHandler(eventHandler, typeof(QRBarcodeQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(PictureQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(TextListQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(InterviewOnClientCreated));

            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(AnswersRemoved));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(TextQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericIntegerQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericRealQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(DateTimeQuestionAnswered));

            bus.RegisterHandler(answerOptionsForCascadingQuestionsDenormalizer, typeof(AnswersRemoved));
            bus.RegisterHandler(answerOptionsForCascadingQuestionsDenormalizer, typeof(SingleOptionQuestionAnswered));
        }

        private void InitTemplateStorage(InProcessEventBus bus)
        {
            var templateDenoramalizer = new QuestionnaireDenormalizer(
                this.kernel.Get<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                this.kernel.Get<IPlainQuestionnaireRepository>());

            bus.RegisterHandler(templateDenoramalizer, typeof(TemplateImported));
            bus.RegisterHandler(templateDenoramalizer, typeof(QuestionnaireDeleted));
            bus.RegisterHandler(templateDenoramalizer, typeof(PlainQuestionnaireRegistered));
            
            var rosterStructureDenormalizer = new QuestionnaireRosterStructureDenormalizer(
                this.kernel.Get<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(),
                this.kernel.Get<IQuestionnaireRosterStructureFactory>(),
                this.kernel.Get<IPlainQuestionnaireRepository>());

            bus.RegisterHandler(rosterStructureDenormalizer, typeof(TemplateImported));
            bus.RegisterHandler(rosterStructureDenormalizer, typeof(QuestionnaireDeleted));
            bus.RegisterHandler(rosterStructureDenormalizer, typeof(PlainQuestionnaireRegistered));
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
            var dashboardeventHandler = new DashboardDenormalizer(
                this.kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>(),
                this.kernel.Get<IReadSideRepositoryWriter<SurveyDto>>(),
                this.kernel.Get<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                this.kernel.Get<IPlainQuestionnaireRepository>());

            bus.RegisterHandler(dashboardeventHandler, typeof(SynchronizationMetadataApplied));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewDeclaredValid));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewDeclaredInvalid));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewStatusChanged));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewSynchronized));
            bus.RegisterHandler(dashboardeventHandler, typeof(TemplateImported));
            bus.RegisterHandler(dashboardeventHandler, typeof(QuestionnaireDeleted));
            bus.RegisterHandler(dashboardeventHandler, typeof(PlainQuestionnaireRegistered));

            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewOnClientCreated));

            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewerAssigned));
            bus.RegisterHandler(dashboardeventHandler, typeof(SupervisorAssigned));

            bus.RegisterHandler(dashboardeventHandler, typeof(TextQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof(MultipleOptionsQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof(SingleOptionQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof (NumericRealQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof(NumericIntegerQuestionAnswered));
            bus.RegisterHandler(dashboardeventHandler, typeof(DateTimeQuestionAnswered));
        }

        internal static void RegisterEventHandlers(InProcessEventBus bus, IKernel kernel)
        {
            IEnumerable<object> handlers = Enumerable.Distinct<object>(kernel.GetAll(typeof(IEventHandler<>))).ToList();
            foreach (object handler in handlers)
            {
                IEnumerable<Type> ieventHandlers = handler.GetType().GetInterfaces().Where(IsIEventHandlerInterface);
                foreach (Type ieventHandler in ieventHandlers)
                {
                    bus.RegisterHandler(handler, ieventHandler.GenericTypeArguments[0]);
                }
            }
        }

        private static bool IsIEventHandlerInterface(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsInterface && typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IEventHandler<>);
        }

        public override void OnCreate()
        {
            base.OnCreate();

             this.RestoreAppState();

             // initialize app if necessary
            MvxAndroidSetupSingleton.EnsureSingletonAvailable(this);
            MvxSingleton<MvxAndroidSetupSingleton>.Instance.EnsureInitialized();
            NcqrsEnvironment.InitDefaults();

            var basePath = Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Personal))
                ? Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                : Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            
            const string SynchronizationFolder = "SYNC";
            const string InterviewFilesFolder = "InterviewData";
            const string QuestionnaireAssembliesFolder = "QuestionnaireAssemblies";


            this.kernel = new StandardKernel(
                new ServiceLocationModule(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new CapiBoundedContextModule(),
                new AndroidCoreRegistry(),
                new AndroidSharedModule(),
                new FileInfrastructureModule(),
                new AndroidLoggingModule());

            this.kernel.Bind<SyncPackageIdsStorage>().ToSelf().InSingletonScope();
            this.kernel.Bind<ISyncPackageIdsStorage>().To<SyncPackageIdsStorage>();

            this.kernel.Load(new AndroidModelModule(basePath,
                    new[] { SynchronizationFolder, InterviewFilesFolder, QuestionnaireAssembliesFolder}, this.kernel.Get<SyncPackageIdsStorage>()),
                new ErrorReportingModule(pathToTemporaryFolder: basePath),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: true, basePath: basePath,
                    syncDirectoryName: SynchronizationFolder, dataDirectoryName: InterviewFilesFolder,
                    questionnaireAssembliesFolder: QuestionnaireAssembliesFolder));

            CrashManager.Initialize(this);
            CrashManager.AttachSender(() => new FileReportSender("Interviewer", this.kernel.Get<IInfoFileSupplierRegistry>()));
         
            this.kernel.Bind<Context>().ToConstant(this);

            NcqrsEnvironment.SetDefault(ServiceLocator.Current.GetInstance<ILogger>());

            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));

            kernel.Bind<ISnapshottingPolicy>().ToMethod(context => NcqrsEnvironment.Get<ISnapshottingPolicy>());
            kernel.Bind<IAggregateRootCreationStrategy>().ToMethod(context => NcqrsEnvironment.Get<IAggregateRootCreationStrategy>());
            kernel.Bind<IAggregateSnapshotter>().ToMethod(context => NcqrsEnvironment.Get<IAggregateSnapshotter>());

            var bus = new InProcessEventBus(Kernel.Get<IEventStore>());
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            kernel.Bind<IEventBus>().ToConstant(bus).Named("interviewViewBus");

            NcqrsEnvironment.SetDefault(Kernel.Get<ISnapshotStore>());
            NcqrsEnvironment.SetDefault(Kernel.Get<IEventStore>());

            this.kernel.Unbind<IAnswerOnQuestionCommandService>();
            this.kernel.Bind<IAnswerOnQuestionCommandService>().To<AnswerOnQuestionCommandService>().InSingletonScope();
            this.kernel.Bind<IAnswerProgressIndicator>().To<Shared.Android.Controls.ScreenItems.AnswerProgressIndicator>().InSingletonScope();
            this.kernel.Bind<IQuestionViewFactory>().To<DefaultQuestionViewFactory>();
            this.kernel.Bind<INavigationService>().To<NavigationService>().InSingletonScope();


            this.kernel.Unbind<ISyncPackageRestoreService>();
            this.kernel.Bind<ISyncPackageRestoreService>().To<SyncPackageRestoreService>().InSingletonScope();

            this.kernel.Bind<IInterviewerSettings>().To<InterviewerSettings>().InSingletonScope();
            this.kernel.Bind<ISynchronizationService>().To<InterviewerSynchronizationService>().InSingletonScope();

            this.kernel.Bind<ISyncProtocolVersionProvider>().To<SyncProtocolVersionProvider>().InSingletonScope();
            #region register handlers

            var eventHandler =
                new InterviewViewModelDenormalizer(
                    this.kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>(),
                    this.kernel.Get<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                    this.kernel.Get<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(),
                    this.kernel.Get<IQuestionnaireRosterStructureFactory>());

            var answerOptionsForLinkedQuestionsDenormalizer = this.kernel.Get<AnswerOptionsForLinkedQuestionsDenormalizer>();
            var answerOptionsForCascadingQuestionsDenormalizer = this.kernel.Get<AnswerOptionsForCascadingQuestionsDenormalizer>();

            this.RegisterInterviewHandlerInBus(
                bus, 
                eventHandler, 
                answerOptionsForLinkedQuestionsDenormalizer, 
                answerOptionsForCascadingQuestionsDenormalizer);

            this.InitTemplateStorage(bus);

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

            var questionnarieDenormalizer = this.kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>() 
                                                as InMemoryReadSideRepositoryAccessor<InterviewViewModel>;
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