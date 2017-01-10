using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    internal class WebInterviewConfigProvider : IWebInterviewConfigProvider
    {
        private IPlainKeyValueStorage<WebInterviewConfig> configs;

        public WebInterviewConfigProvider(IPlainKeyValueStorage<WebInterviewConfig> configs)
        {
            this.configs = configs;
        }

        public WebInterviewConfig Get(QuestionnaireIdentity identity)
        {
            var webInterviewConfig = this.configs.GetById(identity.ToString());
            return webInterviewConfig ?? new WebInterviewConfig {QuestionnaireId = identity};
        }
    }
}