using AndroidNcqrs.Eventing.Storage.SQLite;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using CAPI.Android.Core.Model.Authorization;
using CAPI.Android.Core.Model.Backup;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Core.Model.FileStorage;
using CAPI.Android.Core.Model.SnapshotStore;
using CAPI.Android.Core.Model.SyncCacher;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using CAPI.Android.Core.Model.ViewModel.Statistics;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core.Services;
using Main.Core.View;
using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace CAPI.Android.Core.Model
{
    public class AndroidModelModule : NinjectModule
    {
        private const string ProjectionStoreName = "Projections";
        private const string EventStoreDatabaseName = "EventStore";

        public override void Load()
        {
            var evenStore = new MvvmCrossSqliteEventStore(EventStoreDatabaseName);
            var snapshotStore = new AndroidSnapshotStore();
            var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>(ProjectionStoreName);
            var bigSurveyStore = new BackupableInMemoryReadSideRepositoryAccessor<CompleteQuestionnaireView>();
            var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>(ProjectionStoreName);
            var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>(ProjectionStoreName);
            var publicStore = new SqliteReadSideRepositoryAccessor<PublicChangeSetDTO>(ProjectionStoreName);
            var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>(ProjectionStoreName);
            var fileSystem = new FileStorageService();
            var changeLogStore = new FileChangeLogStore();
            var syncCacher = new FileSyncCacher();

            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);
            this.Bind<IReadSideRepositoryWriter<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IFilterableReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IReadSideRepositoryWriter<CompleteQuestionnaireView>>().ToConstant(bigSurveyStore);
            this.Bind<IReadSideRepositoryReader<CompleteQuestionnaireView>>().ToConstant(bigSurveyStore);
            this.Bind<IReadSideRepositoryWriter<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IFilterableReadSideRepositoryReader<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IFilterableReadSideRepositoryReader<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IReadSideRepositoryWriter<PublicChangeSetDTO>>().ToConstant(publicStore);
            this.Bind<IFilterableReadSideRepositoryWriter<DraftChangesetDTO>>().ToConstant(draftStore);
            this.Bind<IFileStorageService>().ToConstant(fileSystem);
            this.Bind<IChangeLogManipulator>().To<ChangeLogManipulator>().InSingletonScope();
            this.Bind<IAuthentication>().To<AndroidAuthentication>().InSingletonScope();
            this.Bind<IChangeLogStore>().ToConstant(changeLogStore);
            this.Bind<ISyncCacher>().ToConstant(syncCacher);
            this.Bind<IViewFactory<DashboardInput, DashboardModel>>().To<DashboardFactory>();
            this.Bind<IViewFactory<QuestionnaireScreenInput, CompleteQuestionnaireView>>().To<QuestionnaireScreenViewFactory>();
            this.Bind<IViewFactory<StatisticsInput, StatisticsViewModel>>().To<StatisticsViewFactory>();

#warning bad idea to pass loginStore in backuper
            this.Bind<IBackup>().ToConstant(new DefaultBackup(evenStore, changeLogStore,fileSystem, loginStore, snapshotStore, bigSurveyStore));
        }
    }
}