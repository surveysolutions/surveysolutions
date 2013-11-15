
using Main.Core.View;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.UI.QuestionnaireTester
{
    public class AndroidTesterModelModule : NinjectModule
    {
        public override void Load()
        {
            var evenStore = new InMemoryEventStore(); //MvvmCrossSqliteEventStore(EventStoreDatabaseName);
            var snapshotStore = new InMemoryEventStore();//AndroidSnapshotStore();
            //var denormalizerStore = new SqliteDenormalizerStore(ProjectionStoreName);

            //var loginStore = new SqliteReadSideRepositoryAccessor<LoginDTO>(denormalizerStore);
            //var surveyStore = new SqliteReadSideRepositoryAccessor<SurveyDto>(denormalizerStore);
            //var questionnaireStore = new SqliteReadSideRepositoryAccessor<QuestionnaireDTO>(denormalizerStore);
            //var publicStore = new SqliteReadSideRepositoryAccessor<PublicChangeSetDTO>(denormalizerStore);
            //var draftStore = new SqliteReadSideRepositoryAccessor<DraftChangesetDTO>(denormalizerStore);
            //var fileSystem = new FileStorageService();
            //var interviewMetaInfoFactory = new InterviewMetaInfoFactory(questionnaireStore);
            //var changeLogStore = new FileChangeLogStore(interviewMetaInfoFactory);
            //var syncCacher = new FileSyncCacher();
            //var sharedPreferencesBackup = new SharedPreferencesBackupOperator();

            var templateStore = new InMemoryReadSideRepositoryAccessor<QuestionnaireDocumentVersioned>();
            var propagationStructureStore = new InMemoryReadSideRepositoryAccessor<QuestionnaireRosterStructure>();

            var bigSurveyStore = new InMemoryReadSideRepositoryAccessor<InterviewViewModel>();

            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);

            this.Bind<IReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>().ToConstant(templateStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireRosterStructure>>().ToConstant(propagationStructureStore);
            //this.Bind<IReadSideRepositoryWriter<LoginDTO>>().ToConstant(loginStore);
            //this.Bind<IReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);
            //this.Bind<IFilterableReadSideRepositoryReader<LoginDTO>>().ToConstant(loginStore);
            this.Bind<IReadSideRepositoryWriter<InterviewViewModel>>().ToConstant(bigSurveyStore);
            this.Bind<IReadSideRepositoryReader<InterviewViewModel>>().ToConstant(bigSurveyStore);
            //this.Bind<IReadSideRepositoryWriter<SurveyDto>>().ToConstant(surveyStore);
            //this.Bind<IFilterableReadSideRepositoryReader<SurveyDto>>().ToConstant(surveyStore);
            //this.Bind<IReadSideRepositoryWriter<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            //this.Bind<IFilterableReadSideRepositoryReader<QuestionnaireDTO>>().ToConstant(questionnaireStore);
            //this.Bind<IReadSideRepositoryWriter<PublicChangeSetDTO>>().ToConstant(publicStore);
            //this.Bind<IFilterableReadSideRepositoryWriter<DraftChangesetDTO>>().ToConstant(draftStore);
            //this.Bind<IFileStorageService>().ToConstant(fileSystem);
            //this.Bind<IChangeLogManipulator>().To<ChangeLogManipulator>().InSingletonScope();
            //this.Bind<IDataCollectionAuthentication, IAuthentication>().To<AndroidAuthentication>().InSingletonScope();
            //this.Bind<IChangeLogStore>().ToConstant(changeLogStore);
            //this.Bind<ISyncCacher>().ToConstant(syncCacher);
            //this.Bind<IViewFactory<DashboardInput, DashboardModel>>().To<DashboardFactory>();
            //this.Bind<IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo>>().ToConstant(interviewMetaInfoFactory);

            /*this.Bind<IBackup>()
                .ToConstant(new DefaultBackup(evenStore, changeLogStore, fileSystem, denormalizerStore,
                                              bigSurveyStore, syncCacher, sharedPreferencesBackup, templateStore, propagationStructureStore));*/

        }
    }
}