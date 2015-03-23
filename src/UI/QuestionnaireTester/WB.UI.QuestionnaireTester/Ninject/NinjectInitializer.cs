using System.IO;
using Android.Content;
using Cheesebaron.MvxPlugins.Settings.Interfaces;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using IHS.MvvmCross.Plugins.Keychain;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Sqo;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Files;
using WB.Core.Infrastructure.Ncqrs;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement;
using WB.UI.QuestionnaireTester.Implementation.Services;
using WB.UI.Shared.Android;
using WB.UI.Shared.Android.Controls.ScreenItems;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class NinjectInitializer
    {
        private static void InitInterviewStorage(IKernel kernel, InProcessEventBus bus)
        {
            var eventHandler =
                new InterviewViewModelDenormalizer(
                    kernel.Get<IReadSideRepositoryWriter<InterviewViewModel>>(), kernel.Get<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                    kernel.Get<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(), kernel.Get<IQuestionnaireRosterStructureFactory>());

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

            bus.RegisterHandler(eventHandler, typeof(InterviewForTestingCreated));

            var answerOptionsForLinkedQuestionsDenormalizer = kernel.Get<AnswerOptionsForLinkedQuestionsDenormalizer>();

            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(AnswersRemoved));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(TextQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericIntegerQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(NumericRealQuestionAnswered));
            bus.RegisterHandler(answerOptionsForLinkedQuestionsDenormalizer, typeof(DateTimeQuestionAnswered));

            var answerOptionsForCascadingQuestionsDenormalizer = kernel.Get<AnswerOptionsForCascadingQuestionsDenormalizer>();
           
            bus.RegisterHandler(answerOptionsForCascadingQuestionsDenormalizer, typeof(AnswersRemoved));
            bus.RegisterHandler(answerOptionsForCascadingQuestionsDenormalizer, typeof(SingleOptionQuestionAnswered));
        }

        private static void InitTemplateStorage(IKernel kernel, InProcessEventBus bus)
        {
            var templateDenoramalizer = new QuestionnaireDenormalizer(
                kernel.Get<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                kernel.Get<IPlainQuestionnaireRepository>());

            bus.RegisterHandler(templateDenoramalizer, typeof(TemplateImported));
            bus.RegisterHandler(templateDenoramalizer, typeof(QuestionnaireDeleted));
            bus.RegisterHandler(templateDenoramalizer, typeof(PlainQuestionnaireRegistered));
            
            var propagationStructureDenormalizer = new QuestionnaireRosterStructureDenormalizer(
                kernel.Get<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(),
                kernel.Get<IQuestionnaireRosterStructureFactory>(),
                kernel.Get<IPlainQuestionnaireRepository>());

            bus.RegisterHandler(propagationStructureDenormalizer, typeof(TemplateImported));
            bus.RegisterHandler(propagationStructureDenormalizer, typeof(QuestionnaireDeleted));
            bus.RegisterHandler(propagationStructureDenormalizer, typeof(PlainQuestionnaireRegistered));
        }

        public static void Initialize()
        {
            var basePath = Directory.Exists(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal))
                ? System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal)
                : Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

            var kernel = new StandardKernel(
                new ServiceLocationModule(),
                new InfrastructureModule().AsNinject(),
                new NcqrsModule().AsNinject(),
                new CapiTesterCoreRegistry(),
                new CapiBoundedContextModule(),
                new AndroidSharedModule(),
                new TesterLoggingModule(),
                new AndroidTesterModelModule(),
                new DataCollectionSharedKernelModule(usePlainQuestionnaireRepository: false, basePath: basePath),
                new FileInfrastructureModule());

            kernel.Bind<Context>().ToConstant(Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity);

            NcqrsEnvironment.SetDefault(ServiceLocator.Current.GetInstance<ILogger>());
            NcqrsEnvironment.InitDefaults();

            kernel.Unbind<IQuestionnaireAssemblyFileAccessor>();
            kernel.Bind<IQuestionnaireAssemblyFileAccessor>().To<QuestionnareAssemblyTesterFileAccessor>().InSingletonScope();

            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));

            kernel.Bind<ISnapshottingPolicy>().ToMethod(context => NcqrsEnvironment.Get<ISnapshottingPolicy>());
            kernel.Bind<IAggregateRootCreationStrategy>().ToMethod(context => NcqrsEnvironment.Get<IAggregateRootCreationStrategy>());
            kernel.Bind<IAggregateSnapshotter>().ToMethod(context => NcqrsEnvironment.Get<IAggregateSnapshotter>());

            var bus = new InProcessEventBus(kernel.Get<IEventStore>());
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            //kernel.Bind<IEventBus>().ToConstant(bus);
            kernel.Bind<IEventBus>().ToConstant(bus).Named("interviewViewBus");

            NcqrsEnvironment.SetDefault<IEventStore>(kernel.Get<IEventStore>());

            kernel.Unbind<IAnswerOnQuestionCommandService>();
            kernel.Bind<IAnswerOnQuestionCommandService>().To<AnswerOnQuestionCommandService>().InSingletonScope();
            kernel.Bind<IAnswerProgressIndicator>().To<AnswerProgressIndicator>().InSingletonScope();
            kernel.Bind<IQuestionViewFactory>().To<DefaultQuestionViewFactory>();

            kernel.Bind<IAuthentication>().To<DesignerAuthentication>();
            kernel.Bind<IPrincipal>().ToConstant(new Principal(Mvx.Resolve<IKeychain>(), Mvx.Resolve<ISettings>()));
            kernel.Bind(typeof(IQueryablePlainStorageAccessor<>)).To(typeof(SiaqoDbAccessor<>)).InSingletonScope();
            kernel.Bind<IDocumentSerializer>().To<StorageSerializer>().InSingletonScope();
            kernel.Bind<IApplicationSettings>().To<ApplicationSettings>().InSingletonScope();
            
            #region register handlers

            InitInterviewStorage(kernel, bus);
            InitTemplateStorage(kernel, bus);

            #endregion
        }
    }
}