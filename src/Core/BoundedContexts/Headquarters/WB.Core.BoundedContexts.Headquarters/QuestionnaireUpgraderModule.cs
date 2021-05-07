using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.Infrastructure.Modularity;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class QuestionnaireUpgraderModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IQuestionnaireUpgradeService, QuestionnaireUpgradeService>();
        }
    }
}
