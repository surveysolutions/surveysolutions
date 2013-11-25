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
            var evenStore = new InMemoryEventStore();
            var snapshotStore = new InMemoryEventStore();
            
            var templateStore = new InMemoryReadSideRepositoryAccessor<QuestionnaireDocumentVersioned>();
            var propagationStructureStore = new InMemoryReadSideRepositoryAccessor<QuestionnaireRosterStructure>();

            var bigSurveyStore = new InMemoryReadSideRepositoryAccessor<InterviewViewModel>();

            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);

            this.Bind<IReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>().ToConstant(templateStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireRosterStructure>>().ToConstant(propagationStructureStore);
            
            this.Bind<IReadSideRepositoryWriter<InterviewViewModel>>().ToConstant(bigSurveyStore);
            this.Bind<IReadSideRepositoryReader<InterviewViewModel>>().ToConstant(bigSurveyStore);
            
        }
    }
}