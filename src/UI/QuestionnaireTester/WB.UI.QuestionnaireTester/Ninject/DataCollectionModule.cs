using Ninject.Modules;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class DataCollectionModule: NinjectModule
    {
        public override void Load()
        {
            this.Bind<IInterviewExpressionStateUpgrader>().To<InterviewExpressionStateUpgrader>().InSingletonScope();
            this.Bind<IInterviewExpressionStatePrototypeProvider>().To<InterviewExpressionStatePrototypeProvider>();
        }
    }
}