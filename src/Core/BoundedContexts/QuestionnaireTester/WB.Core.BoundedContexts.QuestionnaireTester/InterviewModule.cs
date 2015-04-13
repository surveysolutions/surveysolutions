using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader.Implementation;
using WB.Core.Infrastructure.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.QuestionnaireTester
{
    public class InterviewModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IInterviewStateFullViewModelFactory>().To<InterviewStateFullViewModelFactory>()
                .WithConstructorArgument("plainStorageInterviewAccessor", _ => new InMemoryPlainStorageAccessor<InterviewModel>());
        }
    }
}