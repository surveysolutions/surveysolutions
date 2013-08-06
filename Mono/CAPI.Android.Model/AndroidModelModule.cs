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
using CAPI.Android.Settings;
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
            var denormalizerStore = new SqliteDenormalizerStore(ProjectionStoreName);
            var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>(denormalizerStore);
            var bigSurveyStore = new BackupableInMemoryReadSideRepositoryAccessor<CompleteQuestionnaireView>();
            var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>(denormalizerStore);
            var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>(denormalizerStore);
            var publicStore = new SqliteReadSideRepositoryAccessor<PublicChangeSetDTO>(denormalizerStore);
            var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>(denormalizerStore);
            var fileSystem = new FileStorageService();
            var changeLogStore = new FileChangeLogStore();
            var syncCacher = new FileSyncCacher();
            var sharedPreferencesBackup = new SharedPreferencesBackupOperator();


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

            this.Bind<IBackup>().ToConstant(new DefaultBackup(evenStore, changeLogStore, fileSystem, denormalizerStore, snapshotStore,
                bigSurveyStore, syncCacher, sharedPreferencesBackup));
        }
    }
}