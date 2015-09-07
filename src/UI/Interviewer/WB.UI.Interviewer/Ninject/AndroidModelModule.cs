using System.Collections.Generic;
using Android.OS;
using Main.Core.Documents;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Interviewer;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Implementation.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.InterviewMetaInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation.Storage;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.WriteSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.UI.Interviewer.Backup;
using WB.UI.Interviewer.FileStorage;
using WB.UI.Interviewer.Implementations.DenormalizerStorage;
using WB.UI.Interviewer.Implementations.PlainStorage;
using WB.UI.Interviewer.Implementations.Services;
using WB.UI.Interviewer.ReadSideStore;
using WB.UI.Interviewer.SharedPreferences;
using WB.UI.Interviewer.Syncronization;
using WB.UI.Interviewer.ViewModel.Dashboard;
using WB.UI.Interviewer.ViewModel.InterviewMetaInfo;
using WB.UI.Interviewer.ViewModel.Login;
using WB.UI.Interviewer.ViewModel.Synchronization;

namespace WB.UI.Interviewer.Ninject
{
    public class AndroidModelModule : NinjectModule
    {
        private const string ProjectionStoreName = "Projections";
        private const string EventStoreDatabaseName = "EventStore";
        private const string PlainStoreName = "PlainStore";
        private readonly string basePath;
        private readonly string[] foldersToBackup;
        private readonly IBackupable[] globalBackupables;
        private readonly IWriteSideCleanerRegistry writeSideCleanerRegistry;

        public AndroidModelModule(string basePath, string[] foldersToBackup, IWriteSideCleanerRegistry writeSideCleanerRegistry, params IBackupable[] globalBackupables)
        {
            this.basePath = basePath;
            this.foldersToBackup = foldersToBackup;
            this.writeSideCleanerRegistry = writeSideCleanerRegistry;
            this.globalBackupables = globalBackupables;
        }

        public override void Load()
        {
            var evenStore = new MvvmCrossSqliteEventStore(EventStoreDatabaseName, this.writeSideCleanerRegistry);
            var snapshotStore = new InMemoryCachedSnapshotStore(this.writeSideCleanerRegistry);
            var denormalizerStore = new SqliteDenormalizerStore(ProjectionStoreName);
            var plainStore = new SqlitePlainStore(PlainStoreName);
            var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>(denormalizerStore);
            var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>(denormalizerStore);
            var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>(denormalizerStore);
            var publicStore = new SqliteReadSideRepositoryAccessor<PublicChangeSetDTO>(denormalizerStore);
            var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>(denormalizerStore);
            var plainQuestionnaireStore = new SqlitePlainStorageAccessor<QuestionnaireDocument>(plainStore);
            var questionnaireModelStore = new QuestionnaireModelRepository(new SqlitePlainStorageAccessor<QuestionnaireModel>(plainStore));
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

            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);
            this.Bind<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>().ToConstant(templateStore);
            this.Bind<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>().ToConstant(propagationStructureStore);
            this.Bind<IReadSideRepositoryWriter<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IFilterableReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IReadSideRepositoryWriter<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IFilterableReadSideRepositoryReader<SurveyDto>>().ToConstant(surveyStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IFilterableReadSideRepositoryReader<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            this.Bind<IReadSideRepositoryWriter<PublicChangeSetDTO>>().ToConstant(publicStore);
            this.Bind<IFilterableReadSideRepositoryWriter<DraftChangesetDTO>>().ToConstant(draftStore);

            this.Bind<IPlainKeyValueStorage<QuestionnaireDocument>>().ToConstant(plainQuestionnaireStore);
            this.Bind<IPlainKeyValueStorage<QuestionnaireModel>>().ToConstant(questionnaireModelStore);

            this.Bind<IFileStorageService>().ToConstant(fileSystem);
            this.Bind<IChangeLogManipulator>().To<ChangeLogManipulator>().InSingletonScope();
            this.Bind<IChangeLogStore>().ToConstant(changeLogStore);
            this.Bind<ICapiSynchronizationCacheService>().ToConstant(syncCacher);
            this.Bind<IViewFactory<DashboardInput, DashboardModel>>().To<DashboardFactory>();
            this.Bind<IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo>>().ToConstant(interviewMetaInfoFactory);
            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope();
            this.Bind<SqlitePlainStore>().ToConstant(plainStore);

            var backupable = new List<IBackupable>()
            {
                    evenStore, changeLogStore, fileSystem, denormalizerStore, plainStore,
                    syncCacher, sharedPreferencesBackup, templateStore, propagationStructureStore
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