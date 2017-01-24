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

        public void Start(QuestionnaireIdentity questionnaireId, Guid responsible, bool useCaptcha)
        {
            var webInterviewConfig = this.configs.GetById(questionnaireId.ToString());
            if (webInterviewConfig == null)
            {
                webInterviewConfig = new WebInterviewConfig();
                webInterviewConfig.QuestionnaireId = questionnaireId;
            }

            webInterviewConfig.Started = true;
            webInterviewConfig.ResponsibleId = responsible;
            webInterviewConfig.UseCaptcha = useCaptcha;

            this.configs.Store(webInterviewConfig, questionnaireId.ToString());
        }
    }
}