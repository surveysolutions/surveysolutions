using AndroidNcqrs.Eventing.Storage.SQLite;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Core.Supervisor.Denormalizer;
using Core.Supervisor.Views;
using Main.Core;
using Main.Core.Documents;
using Main.Core.EventHandlers;
using Main.Core.Events.Questionnaire;
using Main.Core.Events.Questionnaire.Completed;
using Main.Core.Events.User;
using Microsoft.Practices.ServiceLocation;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Storage;
using Ncqrs.Eventing.Storage.RavenDB;
using Ninject;
using Ninject.Modules;
using NinjectAdapter;
using Raven.Client.Document;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.EventHandler;
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
            var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>(ProjectionStoreName);
            var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>(ProjectionStoreName);
            var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>(ProjectionStoreName);
            var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>(ProjectionStoreName);
            var changeLogStore = new FileChangeLogStore();

            var eventStore = new CapiDataGeneratorEventStore(capiEvenStore,
                new RavenDBEventStore(this.Kernel.Get<DocumentStore>(), 50));

            this.Bind<IEventStore>().ToConstant(eventStore);
            this.Bind<IStreamableEventStore>().ToConstant(eventStore);

            this.Bind<IReadSideRepositoryWriter<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IReadSideRepositoryWriter<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IFilterableReadSideRepositoryWriter<DraftChangesetDTO>>().ToConstant(draftStore);
            this.Bind<IChangeLogManipulator>().To<ChangeLogManipulator>().InSingletonScope();
            this.Bind<IChangeLogStore>().ToConstant(changeLogStore);

            this.Bind<IReadSideRepositoryReader<UserDocument>>().To<RavenReadSideRepositoryReader<UserDocument>>();
            this.Bind<IQueryableReadSideRepositoryReader<UserDocument>>().To<RavenReadSideRepositoryReader<UserDocument>>();
            this.Bind<IReadSideRepositoryWriter<UserDocument>>().To<RavenReadSideRepositoryWriter<UserDocument>>();
            this.Bind<IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>>().To<RavenReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>>();
            this.Bind<IReadSideRepositoryWriter<QuestionnaireDocument>>().To<RavenReadSideRepositoryWriter<QuestionnaireDocument>>();

            this.Bind<IBackup>().ToConstant(new DefaultBackup(capiEvenStore, changeLogStore, loginStore));

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

            InitSupervisorStorage(bus);

            #endregion

        }

        private void InitSupervisorStorage(InProcessEventBus bus)
        {
            this.Unbind<IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>>();
            this.Unbind<IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>>();
            this.Bind<IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument>, IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument>>()
                .To<RavenReadSideRepositoryWriterWithCacheAndZip<CompleteQuestionnaireStoreDocument>>().InSingletonScope();
            

            var usereventHandler = Kernel.Get<Core.Supervisor.Denormalizer.UserDenormalizer>();
            bus.RegisterHandler(usereventHandler, typeof(NewUserCreated));

            var completeQuestionnaireHandler = Kernel.Get<CompleteQuestionnaireDenormalizer>();
            bus.RegisterHandler(completeQuestionnaireHandler, typeof(NewCompleteQuestionnaireCreated));
            bus.RegisterHandler(completeQuestionnaireHandler, typeof(QuestionnaireStatusChanged));
            bus.RegisterHandler(completeQuestionnaireHandler, typeof(QuestionnaireAssignmentChanged));

            var questionnaireHandler = Kernel.Get<QuestionnaireDenormalizer>();
            bus.RegisterHandler(questionnaireHandler, typeof(TemplateImported));
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