using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.FileStorage;
using Cirrious.MvvmCross.Droid.Platform;
using Main.Core.Events.Questionnaire;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Mono.Android.Crasher;
using Mono.Android.Crasher.Attributes;
using Mono.Android.Crasher.Data.Submit;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ninject;
using WB.Core.BoundedContexts.Capi;
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

namespace WB.UI.QuestionnaireTester
{
    #if RELEASE 
    [Application(Debuggable=false)] 
    #else
    [Application(Debuggable = true)]
    #endif
    [Crasher(UseCustomData = false)]
    public class CapiTesterApplication : Application
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
        
        public static IKernel Kernel
        {
            get
            {
                if (Context == null)
                    return null;
                var capiApp = Context.ApplicationContext as CapiTesterApplication;
                if (capiApp == null)
                    return null;
                return capiApp.kernel;
            }
        }


        #endregion


        protected CapiTesterApplication(IntPtr javaReference, JniHandleOwnership transfer)
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
            
            var propagationStructureDenormalizer =
                new QuestionnaireRosterStructureDenormalizer(
                    this.kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>>());

            bus.RegisterHandler(propagationStructureDenormalizer, typeof(TemplateImported));
        }

        

       /* private void InitUserStorage(InProcessEventBus bus)
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
            
        }*/

        
        public override void OnCreate()
        {
            base.OnCreate();

            CrashManager.Initialize(this);
            CrashManager.AttachSender(() => new FileReportSender("Capi.Tester"));
            this.RestoreAppState();

             // initialize app if necessary
            MvxAndroidSetupSingleton.EnsureSingletonAvailable(this);
            MvxAndroidSetupSingleton.Instance.EnsureInitialized();

            this.kernel = new StandardKernel(
                new CapiBoundedContextModule(),
                //new AndroidCoreRegistry(),
                new AndroidTesterModelModule(),
                new AndroidLoggingModule(),
                new DataCollectionSharedKernelModule(),
                new ExpressionProcessorModule());

            this.kernel.Bind<Context>().ToConstant(this);
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.kernel));
            this.kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);

           /* NcqrsInit.Init(this.kernel);
       
            NcqrsEnvironment.SetDefault<ISnapshotStore>(Kernel.Get<ISnapshotStore>());
            NcqrsEnvironment.SetDefault(NcqrsEnvironment.Get<IEventStore>() as IStreamableEventStore);
            var domainrepository = new DomainRepository(NcqrsEnvironment.Get<IAggregateRootCreationStrategy>(), NcqrsEnvironment.Get<IAggregateSnapshotter>());
            this.kernel.Bind<IDomainRepository>().ToConstant(domainrepository);
            this.kernel.Bind<ICommandService>().ToConstant(CommandService);

            this.kernel.Unbind<IAnswerOnQuestionCommandService>();
            this.kernel.Bind<IAnswerOnQuestionCommandService>().To<AnswerOnQuestionCommandService>().InSingletonScope();
            this.kernel.Bind<IQuestionViewFactory>().To<DefaultQuestionViewFactory>();
            */
            #region register handlers

           /* var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;

            this.InitTemplateStorage(bus);

            this.InitInterviewStorage(bus);*/

/*
            this.InitUserStorage(bus);

            this.InitDashboard(bus);
*/
            
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
            /*this.ClearAllBackStack<SplashScreen>();

            var questionnarieDenormalizer =
                this.kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>() as
                InMemoryReadSideRepositoryAccessor<InterviewViewModel>;
            if (questionnarieDenormalizer != null)
                questionnarieDenormalizer.Clear();*/
        }

        private IKernel kernel;

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }
    }
}