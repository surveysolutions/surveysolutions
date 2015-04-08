using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader.Implimentation;

namespace WB.Core.BoundedContexts.QuestionnaireTester
{
    public class InterviewModule : NinjectModule
    {
        public override void Load()
        {
            Bind<IInterviewStateFullViewModelFactory>().To<InterviewInterviewStateFullViewModelFactory>().InSingletonScope();
        }
    }
}