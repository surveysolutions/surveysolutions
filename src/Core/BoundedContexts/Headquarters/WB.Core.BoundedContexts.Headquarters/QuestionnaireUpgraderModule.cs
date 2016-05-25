using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class QuestionnaireUpgraderModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IQuestionnaireUpgradeService>().To<QuestionnaireUpgradeService>();
        }
    }
}
