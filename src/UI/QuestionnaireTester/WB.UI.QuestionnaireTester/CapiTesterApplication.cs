using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.Content;
using Android.Runtime;
using Cirrious.MvvmCross.Droid.Platform;
using Main.Core;
using Main.Core.Events.Questionnaire;
using Main.Core.View;
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
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Rest.Android;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.EventHandler;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.ExpressionProcessor;
using WB.UI.QuestionnaireTester.Authentication;
using WB.UI.QuestionnaireTester.Services;
using WB.UI.Shared.Android.Controls.ScreenItems;

namespace WB.UI.QuestionnaireTester
{
#if DEBUG 
    [Application(Debuggable=true)] 
#else
    [Application(Debuggable = false)]
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

        public static DesignerAuthentication DesignerMembership
        {
            get { return Kernel.Get<DesignerAuthentication>(); }
        }

        public static DesignerService DesignerServices
        {
            get { return Kernel.Get<DesignerService>(); }
        }

        private const string DesignerPath = "DesignerPath";

        public static string GetPathToDesigner()
        {
            ISharedPreferences prefs = Context.GetSharedPreferences(Context.Resources.GetString(Resource.String.ApplicationName),
                FileCreationMode.Private);
            return prefs.GetString(DesignerPath, Context.Resources.GetString(Resource.String.DesignerPath));
        }

        public static void SetPathToDesigner(string path)
        {
            ISharedPreferences prefs = Application.Context.GetSharedPreferences(Context.Resources.GetString(Resource.String.ApplicationName),
             FileCreationMode.Private);
            ISharedPreferencesEditor prefEditor = prefs.Edit();
            prefEditor.PutString(DesignerPath, path);
            prefEditor.Commit();
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
                    this.kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>>(), this.kernel.Get<IQuestionnaireRosterStructureFactory>());

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
            bus.RegisterHandler(eventHandler, typeof (GroupsDisabled));
            bus.RegisterHandler(eventHandler, typeof (GroupsEnabled));
            bus.RegisterHandler(eventHandler, typeof (QuestionDisabled));
            bus.RegisterHandler(eventHandler, typeof (QuestionEnabled));
            bus.RegisterHandler(eventHandler, typeof (QuestionsDisabled));
            bus.RegisterHandler(eventHandler, typeof (QuestionsEnabled));
            bus.RegisterHandler(eventHandler, typeof (AnswerDeclaredInvalid));
            bus.RegisterHandler(eventHandler, typeof (AnswerDeclaredValid));
            bus.RegisterHandler(eventHandler, typeof (AnswersDeclaredInvalid));
            bus.RegisterHandler(eventHandler, typeof (AnswersDeclaredValid));
            bus.RegisterHandler(eventHandler, typeof(AnswerCommented));
            bus.RegisterHandler(eventHandler, typeof(InterviewCompleted));
            bus.RegisterHandler(eventHandler, typeof(InterviewRestarted));
            bus.RegisterHandler(eventHandler, typeof(GroupPropagated));
            bus.RegisterHandler(eventHandler, typeof(RosterRowAdded));
            bus.RegisterHandler(eventHandler, typeof(RosterRowRemoved));
            bus.RegisterHandler(eventHandler, typeof(RosterInstancesAdded));
            bus.RegisterHandler(eventHandler, typeof(RosterInstancesRemoved));
            bus.RegisterHandler(eventHandler, typeof(SynchronizationMetadataApplied));
            bus.RegisterHandler(eventHandler, typeof(GeoLocationQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(AnswerRemoved));
            bus.RegisterHandler(eventHandler, typeof(AnswersRemoved));
            bus.RegisterHandler(eventHandler, typeof(SingleOptionLinkedQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(MultipleOptionsLinkedQuestionAnswered));
            
            bus.RegisterHandler(eventHandler, typeof(RosterRowTitleChanged));
            bus.RegisterHandler(eventHandler, typeof(RosterInstancesTitleChanged));
            bus.RegisterHandler(eventHandler, typeof(QRBarcodeQuestionAnswered));
            bus.RegisterHandler(eventHandler, typeof(TextListQuestionAnswered));

            var answerOptionsForLinkedQuestionsDenormalizer = this.kernel.Get<AnswerOptionsForLinkedQuestionsDenormalizer>();

            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(AnswerRemoved));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(AnswersRemoved));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(TextQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericIntegerQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericRealQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(DateTimeQuestionAnswered));

            bus.RegisterHandler(eventHandler, typeof(InterviewForTestingCreated));
        }

        private void InitTemplateStorage(InProcessEventBus bus)
        {
            var templateDenoramalizer = new QuestionnaireDenormalizer(
                this.kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>(),
                this.kernel.Get<IPlainQuestionnaireRepository>());

            bus.RegisterHandler(templateDenoramalizer, typeof(TemplateImported));
            bus.RegisterHandler(templateDenoramalizer, typeof(QuestionnaireDeleted));
            bus.RegisterHandler(templateDenoramalizer, typeof(PlainQuestionnaireRegistered));
            
            var propagationStructureDenormalizer = new QuestionnaireRosterStructureDenormalizer(
                this.kernel.Get<IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>>(),
                this.kernel.Get<IQuestionnaireRosterStructureFactory>(),
                this.kernel.Get<IPlainQuestionnaireRepository>());

            bus.RegisterHandler(propagationStructureDenormalizer, typeof(TemplateImported));
            bus.RegisterHandler(propagationStructureDenormalizer, typeof(QuestionnaireDeleted));
            bus.RegisterHandler(propagationStructureDenormalizer, typeof(PlainQuestionnaireRegistered));
        }

        public override void OnCreate()
        {
            base.OnCreate();

            CrashManager.Initialize(this);
            CrashManager.AttachSender(() => new FileReportSender("Capi.Tester"));
            //this.RestoreAppState();

            // initialize app if necessary
            MvxAndroidSetupSingleton.EnsureSingletonAvailable(this);
            MvxAndroidSetupSingleton.Instance.EnsureInitialized();


            this.kernel = new StandardKernel(
                new CapiTesterCoreRegistry(),
                new CapiBoundedContextModule(),
                new AndroidTesterModelModule(),
                new TesterLoggingModule(),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: false),
                new RestAndroidModule(),
                new ExpressionProcessorModule());

            this.kernel.Bind<IAuthentication, DesignerAuthentication>().ToConstant(new DesignerAuthentication());
            this.kernel.Bind<DesignerService>().ToConstant(new DesignerService());
            
            this.kernel.Bind<Context>().ToConstant(this);

            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(this.kernel));
            this.kernel.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);

            NcqrsInit.Init(this.kernel);

            NcqrsEnvironment.SetDefault<ISnapshotStore>(Kernel.Get<ISnapshotStore>());
            NcqrsEnvironment.SetDefault<IEventStore>(Kernel.Get<IEventStore>());
            var domainrepository = new DomainRepository(NcqrsEnvironment.Get<IAggregateRootCreationStrategy>(), NcqrsEnvironment.Get<IAggregateSnapshotter>());
            this.kernel.Bind<IDomainRepository>().ToConstant(domainrepository);
            this.kernel.Bind<ICommandService>().ToConstant(CommandService);

            this.kernel.Unbind<IAnswerOnQuestionCommandService>();
            this.kernel.Bind<IAnswerOnQuestionCommandService>().To<AnswerOnQuestionCommandService>().InSingletonScope();
            this.kernel.Bind<IAnswerProgressIndicator>().To<AnswerProgressIndicator>().InSingletonScope();
            this.kernel.Bind<IQuestionViewFactory>().To<DefaultQuestionViewFactory>();

            #region register handlers

            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;
            this.kernel.Bind<IEventBus>().ToConstant(bus).Named("interviewViewBus");

            this.InitInterviewStorage(bus);
            this.InitTemplateStorage(bus);
            

            #endregion
        }

        private IKernel kernel;

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            GC.Collect();
        }
    }

    public class CapiTesterCoreRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return
                Enumerable.Concat(base.GetAssembliesForRegistration(), new[] { typeof(ImportFromDesignerForTester).Assembly, this.GetType().Assembly });
        }
    }
}