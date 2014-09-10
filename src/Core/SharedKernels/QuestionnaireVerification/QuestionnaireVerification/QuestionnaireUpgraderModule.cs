using Ninject.Modules;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;

namespace WB.Core.SharedKernels.QuestionnaireVerification
{
    public class QuestionnaireUpgraderModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IQuestionnaireUpgradeService>().To<QuestionnaireUpgradeService>();
        }
    }
}
