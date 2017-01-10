using System;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    internal class WebInterviewConfigurator : IWebInterviewConfigurator
    {
        private readonly IPlainKeyValueStorage<WebInterviewConfig> configs;

        public WebInterviewConfigurator(IPlainKeyValueStorage<WebInterviewConfig> configs)
        {
            if (configs == null) throw new ArgumentNullException(nameof(configs));

            this.configs = configs;
        }

        public void Start(QuestionnaireIdentity questionnaire, Guid responsible)
        {
            var webInterviewConfig = this.configs.GetById(questionnaire.ToString());
            if (webInterviewConfig == null)
            {
                webInterviewConfig = new WebInterviewConfig();
                webInterviewConfig.QuestionnaireId = questionnaire;
            }

            webInterviewConfig.Started = true;
            webInterviewConfig.ResponsibleId = responsible;

            this.configs.Store(webInterviewConfig, questionnaire.QuestionnaireId.ToString());
        }
    }
}