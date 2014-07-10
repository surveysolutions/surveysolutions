using Ninject.Modules;
using WB.Core.SharedKernels.QuestionnaireUpgrader.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireUpgrader.Services;

namespace WB.Core.SharedKernels.QuestionnaireUpgrader
{
    public class QuestionnaireUpgraderModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IQuestionnaireUpgradeService>().To<QuestionnaireUpgradeService>();
        }
    }
}
