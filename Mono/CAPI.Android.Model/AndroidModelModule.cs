using System.Collections.Generic;
using System.IO;
using Android.OS;
using AndroidNcqrs.Eventing.Storage.SQLite;
using AndroidNcqrs.Eventing.Storage.SQLite.DenormalizerStorage;
using AndroidNcqrs.Eventing.Storage.SQLite.PlainStorage;
using CAPI.Android.Core.Model.Authorization;
using CAPI.Android.Core.Model.Backup;
using CAPI.Android.Core.Model.FileStorage;
using CAPI.Android.Core.Model.ReadSideStore;
using CAPI.Android.Core.Model.SnapshotStore;
using CAPI.Android.Core.Model.Synchronization;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using CAPI.Android.Core.Model.ViewModel.InterviewMetaInfo;
using CAPI.Android.Core.Model.ViewModel.Login;
using CAPI.Android.Core.Model.ViewModel.Synchronization;
using CAPI.Android.Settings;
using Main.Core.Documents;
using Main.Core.View;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services;
using WB.Core.BoundedContexts.Capi.Synchronization.Services;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.InterviewMetaInfo;
using WB.Core.BoundedContexts.Capi;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.Backup;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.QuestionnaireAssembly;

namespace CAPI.Android.Core.Model
{
    public class AndroidModelModule : NinjectModule
    {
        private const string ProjectionStoreName = "Projections";
        private const string EventStoreDatabaseName = "EventStore";
        private const string PlainStoreName = "PlainStore";
        private readonly string basePath;
        private readonly string[] foldersToBackup;

        public AndroidModelModule(string basePath, string[] foldersToBackup)
        {
            this.basePath = basePath;
            this.foldersToBackup = foldersToBackup;
        }

        public override void Load()
        {
            var evenStore = new MvvmCrossSqliteEventStore(EventStoreDatabaseName);
            var snapshotStore = new FileBasedSnapshotStore();
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
            var changeLogStore = new FileChangeLogStore(interviewMetaInfoFactory);
            var syncCacher = new FileCapiSynchronizationCacheService();
            var sharedPreferencesBackup = new SharedPreferencesBackupOperator();
            var templateStore = new FileReadSideRepositoryWriter<QuestionnaireDocumentVersioned>();
            var propagationStructureStore = new FileReadSideRepositoryWriter<QuestionnaireRosterStructure>();

            var bigSurveyStore = new BackupableInMemoryReadSideRepositoryAccessor<InterviewViewModel>();

            var assemblyFileAccessor = new QuestionnareAssemblyCapiFileAccessor(this.Kernel.Get<IFileSystemAccessor>());
            var stateProvider = new InterviewExpressionStateProvider(assemblyFileAccessor);

            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>().ToConstant(templateStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireRosterStructure>>().ToConstant(propagationStructureStore);
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

            this.Bind<IQuestionnareAssemblyFileAccessor>().ToConstant(assemblyFileAccessor);
            this.Bind<IInterviewExpressionStateProvider>().ToConstant(stateProvider);

            var backupable = new List<IBackupable>(){
                    evenStore, changeLogStore, fileSystem, denormalizerStore, plainStore,
                    bigSurveyStore, syncCacher, sharedPreferencesBackup, templateStore, propagationStructureStore, assemblyFileAccessor
                };

            foreach (var folderToBackup in foldersToBackup)
            {
                backupable.Add(new FolderBackupable(basePath, folderToBackup));
            }

            this.Bind<IBackup>()
                .To<DefaultBackup>()
                .InSingletonScope()
                .WithConstructorArgument("basePath", Environment.ExternalStorageDirectory.AbsolutePath)
                .WithConstructorArgument("backupables", backupable.ToArray());
        }
    }
}