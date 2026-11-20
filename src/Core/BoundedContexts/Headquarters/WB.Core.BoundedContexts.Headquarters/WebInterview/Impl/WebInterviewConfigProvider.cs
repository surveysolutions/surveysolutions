using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    internal class WebInterviewConfigProvider : IWebInterviewConfigProvider
    {
        private readonly IPlainKeyValueStorage<WebInterviewConfig> configs;

        public WebInterviewConfigProvider(IPlainKeyValueStorage<WebInterviewConfig> configs)
        {
            this.configs = configs;
        }

        public WebInterviewConfig Get(QuestionnaireIdentity identity)
        {
            return Get(identity, this.configs.GetById(identity.ToString()));
        }

        public WebInterviewConfig GetUncached(QuestionnaireIdentity identity)
        {
            return Get(identity, this.configs.GetByIdUncached(identity.ToString()));
        }

        private static WebInterviewConfig Get(QuestionnaireIdentity identity, WebInterviewConfig webInterviewConfig)
        {
            return webInterviewConfig ?? new WebInterviewConfig
            {
                QuestionnaireId = identity,
                ReminderAfterDaysIfPartialResponse = 3,
                ReminderAfterDaysIfNoResponse = 3,
            };
        }

        public void Store(QuestionnaireIdentity identity, WebInterviewConfig config)
        {
            this.configs.Store(config, identity.ToString());
        }
    }
}
