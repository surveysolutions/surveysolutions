using Main.Core.Documents;
using Ninject;
using Ninject.Modules;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class PlainStorageInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IPlainKeyValueStorage<QuestionnaireDocument>>().To<InMemoryKeyValueStorage<QuestionnaireDocument>>().InSingletonScope();
            this.Bind<IPlainKeyValueStorage<QuestionnaireModel>>().To<InMemoryKeyValueStorage<QuestionnaireModel>>().InSingletonScope();

            this.Bind<IPlainStorageAccessor<QuestionnaireModel>>().To<InMemoryPlainStorageAccessor<QuestionnaireModel>>().InSingletonScope();

            this.Bind<IPlainQuestionnaireRepository>().To<PlainQuestionnaireRepository>().InSingletonScope();
            this.Bind<IQuestionnaireRepository>().ToConstant<IQuestionnaireRepository>(this.Kernel.Get<IPlainQuestionnaireRepository>());
            this.Bind<IStatefullInterviewRepository>().To<StatefullInterviewRepository>().InSingletonScope();
        }
    }
}