using Ninject.Modules;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer
{
    public class QuestionnaireVerificationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IQuestionnaireVerifier>().To<QuestionnaireVerifier>();
        }
    }
}
