using Main.Core.Documents;
using Main.DenormalizerStorage;
using Ncqrs.Eventing.Storage;
using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
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

            var plainQuestionnaireStore = new InMemoryPlainStorageAccessor<QuestionnaireDocument>();

            this.Bind<IEventStore>().ToConstant(evenStore);
            this.Bind<ISnapshotStore>().ToConstant(snapshotStore);

            this.Bind<IReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>().ToConstant(templateStore);
            this.Bind<IReadSideRepositoryWriter<QuestionnaireRosterStructure>>().ToConstant(propagationStructureStore);
            
            this.Bind<IReadSideRepositoryWriter<InterviewViewModel>>().ToConstant(bigSurveyStore);
            this.Bind<IReadSideRepositoryReader<InterviewViewModel>>().ToConstant(bigSurveyStore);
            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();

            this.Bind<IPlainStorageAccessor<QuestionnaireDocument>>().ToConstant(plainQuestionnaireStore);

            this.Bind<IQuestionnaireAssemblyFileAccessor>().To<QuestionnareAssemblyTesterFileAccessor>().InSingletonScope();

            //this.kernel.Unbind<IInterviewExpressionStateProvider>();
            this.Bind<IInterviewExpressionStatePrototypeProvider>().To<InterviewExpressionStatePrototypeProvider>().InSingletonScope();
        }
    }
}