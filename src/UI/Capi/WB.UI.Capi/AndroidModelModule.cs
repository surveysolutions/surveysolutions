using System.Collections.Generic;

using Android.OS;

using AndroidNcqrs.Eventing.Storage.SQLite;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage;

using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using Main.Core.Documents;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Capi.Views.InterviewMetaInfo;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.UI.Capi.Authorization;
using WB.UI.Capi.Backup;
using WB.UI.Capi.FileStorage;
using WB.UI.Capi.ReadSideStore;
using WB.UI.Capi.SharedPreferences;
using WB.UI.Capi.SnapshotStore;
using WB.UI.Capi.Syncronization;
using WB.UI.Capi.ViewModel.Dashboard;
using WB.UI.Capi.ViewModel.InterviewMetaInfo;

namespace WB.UI.Capi
{
    public class AndroidModelModule : NinjectModule
    {
        private const string ProjectionStoreName = "Projections";
        private const string EventStoreDatabaseName = "EventStore";
        private const string PlainStoreName = "PlainStore";
        private readonly string basePath;
        private readonly string[] foldersToBackup;
        private readonly IBackupable[] globalBackupables;

        public AndroidModelModule(string basePath, string[] foldersToBackup, params IBackupable[] globalBackupables)
        {
            this.basePath = basePath;
            this.foldersToBackup = foldersToBackup;
            this.globalBackupables = globalBackupables;
        }

        public override void Load()
        {
            var evenStore = new MvvmCrossSqliteEventStore(EventStoreDatabaseName);
            var snapshotStore = new FileBasedSnapshotStore(this.Kernel.Get<IJsonUtils>());
            var denormalizerStore = new SqliteDenormalizerStore(ProjectionStoreName);
            var plainStore = new SqlitePlainStore(PlainStoreName);
            var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>(denormalizerStore);
            var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>(denormalizerStore);
            var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>(denormalizerStore);
            var publicStore = new SqliteReadSideRepositoryAccessor<PublicChangeSetDTO>(denormalizerStore);
            var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>(denormalizerStore);
            var plainQuestionnaireStore = new SqlitePlainStorageAccessor<QuestionnaireDocument>(plainStore);
            var fileSystem = new FileStorageService();
            var interviewMetaInfoFactory = new InterviewMetaInfoFactory(questionnaireStore);

            var changeLogStore = new FileChangeLogStore(
                interviewMetaInfoFactory, 
                this.Kernel.Get<IArchiveUtils>(), 
                this.Kernel.Get<IFileSystemAccessor>(),
                this.Kernel.Get<IJsonUtils>(),
                this.basePath);

            var syncCacher = new FileCapiSynchronizationCacheService(this.Kernel.Get<IFileSystemAccessor>(), this.basePath);
            var sharedPreferencesBackup = new SharedPreferencesBackupOperator();
            var templateStore = new FileReadSideRepositoryWriter<QuestionnaireDocumentVersioned>(this.Kernel.Get<IJsonUtils>());
            var propagationStructureStore = new FileReadSideRepositoryWriter<QuestionnaireRosterStructure>(this.Kernel.Get<IJsonUtils>());

            var bigSurveyStore = new BackupableInMemoryReadSideRepositoryAccessor<InterviewViewModel>();

            NcqrsEnvironment.SetDefault<ISnapshotStore>(snapshotStore);

            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);
            this.Bind<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>().ToConstant(templateStore);
            this.Bind<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>().ToConstant(propagationStructureStore);
            this.Bind<IReadSideRepositoryWriter<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IFilterableReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IReadSideRepositoryWriter<InterviewViewModel>>().ToConstant(bigSurveyStore);
            this.Bind<IReadSideRepositoryReader<InterviewViewModel>>().ToConstant(bigSurveyStore);
            this.Bind<IReadSideRepositoryWriter<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IFilterableReadSideRepositoryReader<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IFilterableReadSideRepositoryReader<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IReadSideRepositoryWriter<PublicChangeSetDTO>>().ToConstant(publicStore);
            this.Bind<IFilterableReadSideRepositoryWriter<DraftChangesetDTO>>().ToConstant(draftStore);
            this.Bind<IPlainStorageAccessor<QuestionnaireDocument>>().ToConstant(plainQuestionnaireStore);
            this.Bind<IFileStorageService>().ToConstant(fileSystem);
            this.Bind<IChangeLogManipulator>().To<ChangeLogManipulator>().InSingletonScope();
            this.Bind<IDataCollectionAuthentication, IAuthentication>().To<AndroidAuthentication>().InSingletonScope();
            this.Bind<IChangeLogStore>().ToConstant(changeLogStore);
            this.Bind<ICapiSynchronizationCacheService>().ToConstant(syncCacher);
            this.Bind<IViewFactory<DashboardInput, DashboardModel>>().To<DashboardFactory>();
            this.Bind<IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo>>().ToConstant(interviewMetaInfoFactory);
            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope();
            this.Bind<SqlitePlainStore>().ToConstant(plainStore);

            var backupable = new List<IBackupable>()
            {
                    evenStore, changeLogStore, fileSystem, denormalizerStore, plainStore,
                    bigSurveyStore, syncCacher, sharedPreferencesBackup, templateStore, propagationStructureStore
            };
            if (this.globalBackupables != null && this.globalBackupables.Length > 0)
            {
                backupable.AddRange(this.globalBackupables);
            }
            foreach (var folderToBackup in this.foldersToBackup)
            {
                backupable.Add(new FolderBackupable(this.basePath, folderToBackup));
            }

            this.Bind<IBackup>()
                .To<DefaultBackup>()
                .InSingletonScope()
                .WithConstructorArgument("basePath", Environment.ExternalStorageDirectory.AbsolutePath)
                .WithConstructorArgument("backupables", backupable.ToArray());
        }
    }
}