using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModelLoader.Implementation;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;

namespace WB.Core.BoundedContexts.QuestionnaireTester
{
    public class InterviewModule : NinjectModule
    {
        public override void Load()
        {
            Bind<InterviewLeftSidePanelViewModel>().ToSelf();
            Bind<IInterviewStateFullViewModelFactory>().To<InterviewStateFullViewModelFactory>();
            Bind<ITesterExpressionsEngineVersionService>().To<TesterExpressionsEngineVersionService>();
        }
    }
}