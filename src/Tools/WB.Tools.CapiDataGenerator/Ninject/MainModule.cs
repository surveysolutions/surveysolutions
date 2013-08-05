using System.IO;
using AndroidNcqrs.Eventing.Storage.SQLite;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core;
using Main.Core.Documents;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.Events.User;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using NinjectAdapter;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.Raven.Implementation.WriteSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Tools.CapiDataGenerator.Models;
using UserDenormalizer = CAPI.Android.Core.Model.EventHandlers.UserDenormalizer;

namespace CapiDataGenerator
{
    public class MainModelModule : NinjectModule
    {
        private const string ProjectionStoreName = "Projections";
        private const string EventStoreDatabaseName = "EventStore";


        public override void Load()
        {
            var capiEvenStore = new MvvmCrossSqliteEventStore(EventStoreDatabaseName);
            var denormalizerStore = new SqliteDenormalizerStore(ProjectionStoreName);
            var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>(denormalizerStore);
            var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>(denormalizerStore);
            var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>(denormalizerStore);
            var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>(denormalizerStore);
            var changeLogStore = new FileChangeLogStore();

            ClearCapiDb(capiEvenStore, denormalizerStore, changeLogStore);

            var eventStore = new CapiDataGeneratorEventStore(capiEvenStore,
                new RavenDBEventStore(this.Kernel.Get<DocumentStoreProvider>().CreateSeparateInstanceForEventStore(), 50));

            this.Bind<IEventStore>().ToConstant(eventStore);
            this.Bind<IStreamableEventStore>().ToConstant(eventStore);

            this.Bind<IReadSideRepositoryWriter<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IReadSideRepositoryWriter<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IFilterableReadSideRepositoryWriter<DraftChangesetDTO>>().ToConstant(draftStore);
            this.Bind<IChangeLogManipulator>().ToConstant(new ChangeLogManipulator(null, draftStore, capiEvenStore,changeLogStore));
            this.Bind<IChangeLogStore>().ToConstant(changeLogStore);

            this.Bind<IReadSideRepositoryReader<UserDocument>>().To<RavenReadSideRepositoryReader<UserDocument>>();
            this.Bind<IQueryableReadSideRepositoryReader<UserDocument>>().To<RavenReadSideRepositoryReader<UserDocument>>();
            this.Bind<IReadSideRepositoryWriter<UserDocument>>().To<RavenReadSideRepositoryWriter<UserDocument>>();
            this.Bind<IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>>().To<RavenReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>>();
            this.Bind<IReadSideRepositoryWriter<QuestionnaireDocument>>().To<RavenReadSideRepositoryWriter<QuestionnaireDocument>>();

            this.Bind<IBackup>().ToConstant(new DefaultBackup(capiEvenStore, changeLogStore, denormalizerStore));

            ServiceLocator.SetLocatorProvider(() => new NinjectServiceLocator(Kernel));
            this.Bind<IServiceLocator>().ToMethod(_ => ServiceLocator.Current);

            NcqrsInit.InitPartial(Kernel);
            this.Bind<ICommandService>().ToConstant(NcqrsEnvironment.Get<ICommandService>());
            NcqrsEnvironment.SetDefault<IStreamableEventStore>(eventStore);
            NcqrsEnvironment.SetDefault<IEventStore>(eventStore);
            
            #region register handlers

            var bus = NcqrsEnvironment.Get<IEventBus>() as InProcessEventBus;

            InitUserStorage(bus);

            InitDashboard(bus);

            InitChangeLog(bus);

            #endregion

        }

        private void ClearCapiDb(params IBackupable[] stores)
        {
            foreach (var store in stores)
            {
                var storePath = store.GetPathToBakupFile();
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
                                          Kernel.Get<IReadSideRepositoryWriter<SurveyDto>>());
            bus.RegisterHandler(dashboardeventHandler, typeof(NewAssigmentCreated));
            bus.RegisterHandler(dashboardeventHandler, typeof(QuestionnaireStatusChanged));
        }

        private void InitChangeLog(InProcessEventBus bus)
        {
            var changeLogHandler = new CommitDenormalizer(Kernel.Get<IChangeLogManipulator>());
            bus.RegisterHandler(changeLogHandler, typeof(NewAssigmentCreated));
            bus.RegisterHandler(changeLogHandler, typeof(QuestionnaireStatusChanged));
        }
    }
}