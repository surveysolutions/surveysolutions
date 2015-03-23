using Main.Core.Documents;
using Main.DenormalizerStorage;
using Ncqrs;
using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester
{
    public class AndroidTesterModelModule : NinjectModule
    {
        public override void Load()
        {
            var evenStore = new InMemoryEventStore();
            var snapshotStore = new InMemoryEventStore();

            NcqrsEnvironment.SetDefault<ISnapshotStore>(snapshotStore);
            
            var templateStore = new InMemoryReadSideRepositoryAccessor<QuestionnaireDocumentVersioned>();
            var propagationStructureStore = new InMemoryReadSideRepositoryAccessor<QuestionnaireRosterStructure>();

            var bigSurveyStore = new InMemoryReadSideRepositoryAccessor<InterviewViewModel>();

            var plainQuestionnaireStore = new InMemoryPlainStorageAccessor<QuestionnaireDocument>();

            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);
            this.Bind<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>().ToConstant(templateStore);
            this.Bind<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>().ToConstant(propagationStructureStore);
            this.Bind<IReadSideRepositoryWriter<InterviewViewModel>>().ToConstant(bigSurveyStore);
            this.Bind<IReadSideRepositoryReader<InterviewViewModel>>().ToConstant(bigSurveyStore);
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<JsonCompressor>();
            this.Bind<IPlainStorageAccessor<QuestionnaireDocument>>().ToConstant(plainQuestionnaireStore);
            this.Bind<IRestServiceSettings>().To<RestServiceSettings>().InSingletonScope();
        }
    }
}