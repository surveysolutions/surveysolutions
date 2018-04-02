using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    internal class WebInterviewConfigurator : IWebInterviewConfigurator
    {
        private readonly IPlainKeyValueStorage<WebInterviewConfig> configs;

        public WebInterviewConfigurator(IPlainKeyValueStorage<WebInterviewConfig> configs)
        {
            this.configs = configs ?? throw new ArgumentNullException(nameof(configs));
        }

        public void Start(QuestionnaireIdentity questionnaireId, bool useCaptcha)
        {
            var webInterviewConfig = this.configs.GetById(questionnaireId.ToString());
            if (webInterviewConfig == null)
            {
                webInterviewConfig = new WebInterviewConfig();
                webInterviewConfig.QuestionnaireId = questionnaireId;
            }

            webInterviewConfig.Started = true;
            webInterviewConfig.UseCaptcha = useCaptcha;
        
            this.configs.Store(webInterviewConfig, questionnaireId.ToString());
        }

        public void UpdateMessages(QuestionnaireIdentity questionnaireId, Dictionary<WebInterviewUserMessages, string> messages)
        {
            var config = this.configs.GetById(questionnaireId.ToString()) ?? new WebInterviewConfig();
            config.CustomMessages = messages;
            this.configs.Store(config, questionnaireId.ToString());
        }

        public void Stop(QuestionnaireIdentity questionnaireId)
        {
            var config = this.configs.GetById(questionnaireId.ToString()) ?? new WebInterviewConfig();
            config.Started = false;
            this.configs.Store(config, questionnaireId.ToString());
        }
    }
}