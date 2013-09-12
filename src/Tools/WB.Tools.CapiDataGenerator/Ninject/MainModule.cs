using System;
using System.IO;
using AndroidNcqrs.Eventing.Storage.SQLite;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.ReadSideStore;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.InterviewMetaInfo;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core;
using Main.Core.Commands;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.User;
using Main.Core.Services;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using NinjectAdapter;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.Infrastructure.Raven.Implementation.WriteSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Tools.CapiDataGenerator.Models;
using UserDenormalizer = CAPI.Android.Core.Model.EventHandlers.UserDenormalizer;

namespace CapiDataGenerator
{
    using WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide;
    using WB.Core.BoundedContexts.Supervisor.Views.Interview;
    using WB.Core.SharedKernels.DataCollection.Implementation.ReadSide;

    public class MainModelModule : NinjectModule
    {
        private const string ProjectionStoreName = "Projections";
        private const string EventStoreDatabaseName = "EventStore";

        private IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> capiTemplateVersionedWriter;

        public override void Load()
        {
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<GZipJsonCompressor>();
            var capiEvenStore = new MvvmCrossSqliteEventStore(EventStoreDatabaseName);
            var denormalizerStore = new SqliteDenormalizerStore(ProjectionStoreName);
            var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>(denormalizerStore);
            var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>(denormalizerStore);
            var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>(denormalizerStore);
            var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>(denormalizerStore);
            var publicStore = new SqliteReadSideRepositoryAccessor<PublicChangeSetDTO>(denormalizerStore);
            var interviewMetaInfoFactory = new InterviewMetaInfoFactory(questionnaireStore);
            var changeLogStore = new FileChangeLogStore(interviewMetaInfoFactory);

            var capiTemplateWriter = new FileReadSideRepositoryWriter<QuestionnaireDocumentVersioned>();

            this.capiTemplateVersionedWriter = new VersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>(capiTemplateWriter);

            ClearCapiDb(capiEvenStore, denormalizerStore, changeLogStore);

            var eventStore = new CapiDataGeneratorEventStore(capiEvenStore,
                new RavenDBEventStore(this.Kernel.Get<DocumentStoreProvider>().CreateSeparateInstanceForEventStore(), 50));

            this.Bind<IEventStore>().ToConstant(eventStore);
            this.Bind<IStreamableEventStore>().ToConstant(eventStore);

            this.Bind<IReadSideRepositoryCleanerRegistry>().To<ReadSideRepositoryCleanerRegistry>().InSingletonScope();

            this.Bind<IReadSideRepositoryWriter<InterviewData>, IReadSideRepositoryReader<InterviewData>>().To<InterviewDataRepositoryWriterWithCache>().InSingletonScope();
            
            this.Bind<IReadSideRepositoryWriter<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IFilterableReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IReadSideRepositoryWriter<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IFilterableReadSideRepositoryReader<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IFilterableReadSideRepositoryReader<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IReadSideRepositoryWriter<PublicChangeSetDTO>>().ToConstant(publicStore);
            this.Bind<IFilterableReadSideRepositoryWriter<DraftChangesetDTO>>().ToConstant(draftStore);
            this.Bind<IChangeLogManipulator>().ToConstant(new ChangeLogManipulator(publicStore, draftStore, capiEvenStore, changeLogStore));
            this.Bind<IChangeLogStore>().ToConstant(changeLogStore);

            this.Bind<IBackup>().ToConstant(new DefaultBackup(capiEvenStore, changeLogStore, denormalizerStore, capiTemplateWriter));
            
            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(Kernel));
            this.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);

            var commandService = new ConcurrencyResolveCommandService(ServiceLocator.Current.GetInstance<ILogger>());
            NcqrsEnvironment.SetDefault(commandService);
            NcqrsInit.InitializeCommandService(Kernel.Get<ICommandListSupplier>(), commandService);
            NcqrsEnvironment.SetDefault(Kernel.Get<IFileStorageService>());
            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));

            var snpshotStore = new InMemoryEventStore();
            // key param for storing im memory
            NcqrsEnvironment.SetDefault<ISnapshotStore>(snpshotStore);

            var bus = new CustomInProcessEventBus(true);
            this.Bind<IViewConstructorEventBus>().ToConstant(bus);
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            this.Bind<IEventBus>().ToConstant(bus);
            NcqrsEnvironment.SetDefault<IStreamableEventStore>(eventStore);
            NcqrsEnvironment.SetDefault<IEventStore>(eventStore);
           
            NcqrsInit.RegisterEventHandlers(bus, Kernel);

            this.Bind<ICommandService>().ToConstant(NcqrsEnvironment.Get<ICommandService>());
            
            #region register handlers

            InitCapiTemplateStorage(bus);
            
            InitUserStorage(bus);

            InitDashboard(bus);

            InitChangeLog(bus);

            #endregion

            var repository = new DomainRepository(NcqrsEnvironment.Get<IAggregateRootCreationStrategy>(), NcqrsEnvironment.Get<IAggregateSnapshotter>());
            this.Bind<IDomainRepository>().ToConstant(repository);
            this.Bind<ISnapshotStore>().ToConstant(NcqrsEnvironment.Get<ISnapshotStore>());
        }

        private void ClearCapiDb(params IBackupable[] stores)
        {
            foreach (var store in stores)
            {
                var storePath = store.GetPathToBakupFile();
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

        private void InitCapiTemplateStorage(InProcessEventBus bus)
        {
            var fileSorage = new QuestionnaireDenormalizer(this.capiTemplateVersionedWriter);
            bus.RegisterHandler(fileSorage, typeof(TemplateImported));
        }

        private void InitUserStorage(InProcessEventBus bus)
        {
            var usereventHandler =
                new UserDenormalizer(Kernel.Get<IReadSideRepositoryWriter<LoginDTO>>());
            bus.RegisterHandler(usereventHandler, typeof(NewUserCreated));
        }

        private void InitDashboard(InProcessEventBus bus)
        {
            var dashboardeventHandler =
                new DashboardDenormalizer(Kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>(),
                                          Kernel.Get<IReadSideRepositoryWriter<SurveyDto>>(),
                                          this.capiTemplateVersionedWriter);

            bus.RegisterHandler(dashboardeventHandler, typeof(SynchronizationMetadataApplied));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewRestarted));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewCompleted));
            bus.RegisterHandler(dashboardeventHandler, typeof(TemplateImported));
            bus.RegisterHandler(dashboardeventHandler, typeof(InterviewSynchronized));
        }

        private void InitChangeLog(InProcessEventBus bus)
        {
            var changeLogHandler = new CommitDenormalizer(Kernel.Get<IChangeLogManipulator>());
            bus.RegisterHandler(changeLogHandler, typeof(InterviewDeclaredInvalid));
            bus.RegisterHandler(changeLogHandler, typeof(InterviewDeclaredValid));
            bus.RegisterHandler(changeLogHandler, typeof(InterviewRestarted));
            bus.RegisterHandler(changeLogHandler, typeof(InterviewSynchronized));
        }
    }
}