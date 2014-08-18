using System;
using System.IO;
using AndroidNcqrs.Eventing.Storage.SQLite;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.ReadSideStore;
using CAPI.Android.Core.Model.Synchronization;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.InterviewMetaInfo;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core;
using Main.Core.Commands;
using Main.Core.Documents;
using Main.Core.View;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using NinjectAdapter;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.ChangeLog;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.EventDispatcher;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Raven;
using WB.Core.Infrastructure.Storage.Raven.Implementation;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.EventHandler;
using WB.Core.SharedKernels.DataCollection.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Tools.CapiDataGenerator.Ninject;
using UserDenormalizer = CAPI.Android.Core.Model.EventHandlers.UserDenormalizer;

namespace CapiDataGenerator
{
    public class MainModelModule : NinjectModule
    {
        private const string ProjectionStoreName = "Projections";
        private const string EventStoreDatabaseName = "EventStore";
        private const string PlainStoreName = "PlainStore";

        private readonly RavenConnectionSettings headquartersSettings;

        private IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> capiTemplateVersionedWriter;

        public MainModelModule(RavenConnectionSettings headquartersSettings)
        {
            this.headquartersSettings = headquartersSettings;
        }

        public override void Load()
        {
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<GZipJsonCompressor>();
            var capiEvenStore = new MvvmCrossSqliteEventStore(EventStoreDatabaseName);
            var denormalizerStore = new SqliteDenormalizerStore(ProjectionStoreName);
            var plainStore = new SqlitePlainStore(PlainStoreName);
            var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>(denormalizerStore);
            var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>(denormalizerStore);
            var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>(denormalizerStore);
            var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>(denormalizerStore);
            var publicStore = new SqliteReadSideRepositoryAccessor<PublicChangeSetDTO>(denormalizerStore);
            var plainQuestionnaireStore = new SqlitePlainStorageAccessor<QuestionnaireDocument>(plainStore);
            var interviewMetaInfoFactory = new InterviewMetaInfoFactory(questionnaireStore);
            var changeLogStore = new FileChangeLogStore(interviewMetaInfoFactory);

            var capiTemplateWriter = new FileReadSideRepositoryWriter<QuestionnaireDocumentVersioned>();
            this.capiTemplateVersionedWriter = new VersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>(capiTemplateWriter);
            
            ClearCapiDb(capiEvenStore, denormalizerStore, plainStore, changeLogStore, capiTemplateWriter);

            var supervisorEventStore = new RavenDBEventStore(
                this.Kernel.Get<DocumentStoreProvider>().CreateSeparateInstanceForEventStore(), 50);

            //manual creation of hq event store
            var storeProvider = new DocumentStoreProvider(this.headquartersSettings);
            var headquartersEventStore = new RavenDBEventStore(storeProvider.CreateSeparateInstanceForEventStore(), 50);

            var eventStore = new CapiDataGeneratorEventStore(capiEvenStore, supervisorEventStore, headquartersEventStore);

            this.Bind<IEventStore>().ToConstant(eventStore);
            this.Bind<IStreamableEventStore>().ToConstant(eventStore);

            this.Bind<IReadSideRepositoryReader<InterviewData>>().To<ReadSideRepositoryReaderWithSequence<InterviewData>>().InSingletonScope();
            
            this.Bind<IReadSideRepositoryWriter<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IFilterableReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IReadSideRepositoryWriter<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IFilterableReadSideRepositoryReader<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IFilterableReadSideRepositoryReader<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IReadSideRepositoryWriter<PublicChangeSetDTO>>().ToConstant(publicStore);
            this.Bind<IFilterableReadSideRepositoryWriter<DraftChangesetDTO>>().ToConstant(draftStore);
            this.Bind<IPlainStorageAccessor<QuestionnaireDocument>>().ToConstant(plainQuestionnaireStore);
            this.Bind<IChangeLogManipulator>().ToConstant(new ChangeLogManipulator(publicStore, draftStore, capiEvenStore, changeLogStore));
            this.Bind<IChangeLogStore>().ToConstant(changeLogStore);

            this.Bind<IBackup>().To<DefaultBackup>().InSingletonScope()
                .WithConstructorArgument("backupables",
                    new IBackupable[]{capiEvenStore, changeLogStore, denormalizerStore, plainStore, capiTemplateWriter});

            this.Bind<IViewFactory<UserListViewInputModel, UserListView>>().To<UserListViewFactory>();
            
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(Kernel));
            this.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);

            var commandService = new ConcurrencyResolveCommandService(ServiceLocator.Current.GetInstance<ILogger>());
            NcqrsEnvironment.SetDefault(commandService);
            NcqrsInit.InitializeCommandService(Kernel.Get<ICommandListSupplier>(), commandService);
            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));

            var snpshotStore = new InMemoryEventStore();
            // key param for storing im memory
            NcqrsEnvironment.SetDefault<ISnapshotStore>(snpshotStore);

            var inProcessEventDispatcher = new CustomInProcessEventDispatcher(true);

            var bus = new NcqrCompatibleEventDispatcher(() => inProcessEventDispatcher);

            this.Bind<IEventDispatcher>().ToConstant(bus);
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            this.Bind<IEventBus>().ToConstant(bus);
            NcqrsEnvironment.SetDefault<IStreamableEventStore>(eventStore);
            NcqrsEnvironment.SetDefault<IEventStore>(eventStore);

            foreach (var handler in Kernel.GetAll(typeof(IEventHandler)))
            {
                bus.Register(handler as IEventHandler);
            }

            this.Bind<ICommandService>().ToConstant(NcqrsEnvironment.Get<ICommandService>());
            
            #region register handlers

            InitCapiTemplateStorage(bus);

            InitUserStorage(bus);

            InitDashboard(bus);


            #endregion

            var repository = new DomainRepository(NcqrsEnvironment.Get<IAggregateRootCreationStrategy>(), NcqrsEnvironment.Get<IAggregateSnapshotter>());
            this.Bind<IDomainRepository>().ToConstant(repository);
            this.Bind<ISnapshotStore>().ToConstant(NcqrsEnvironment.Get<ISnapshotStore>());
        }

        private void ClearCapiDb(params IBackupable[] stores)
        {
            foreach (var store in stores)
            {
                var storePath = store.GetPathToBackupFile();
                try
                {
                    FileAttributes attr = File.GetAttributes(storePath);
                    if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        Directory.Delete(storePath, true);
                        Directory.CreateDirectory(storePath);
                    }
                    else
                    {
                        File.Delete(storePath);
                        File.Create(storePath);
                    }
                }
                catch (Exception)
                {
                }
                
            }
        }

        private void InitCapiTemplateStorage(NcqrCompatibleEventDispatcher bus)
        {
            var fileSorage = new QuestionnaireDenormalizer(this.capiTemplateVersionedWriter, this.Kernel.Get<IPlainQuestionnaireRepository>());
            bus.Register(fileSorage);
        }

        private void InitUserStorage(NcqrCompatibleEventDispatcher bus)
        {
            var usereventHandler =
                new UserDenormalizer(Kernel.Get<IReadSideRepositoryWriter<LoginDTO>>());
            bus.Register(usereventHandler);
        }

        private void InitDashboard(NcqrCompatibleEventDispatcher bus)
        {
            var dashboardeventHandler =
                new DashboardDenormalizer(Kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>(),
                                          Kernel.Get<IReadSideRepositoryWriter<SurveyDto>>(),
                                          this.capiTemplateVersionedWriter,
                                          this.Kernel.Get<IPlainQuestionnaireRepository>());

            bus.Register(dashboardeventHandler);
        }
        
    }
}