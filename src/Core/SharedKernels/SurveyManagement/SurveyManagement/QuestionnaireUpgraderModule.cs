using Ninject.Modules;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement
{
    public class QuestionnaireUpgraderModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IQuestionnaireUpgradeService>().To<QuestionnaireUpgradeService>();
        }
    }
}
