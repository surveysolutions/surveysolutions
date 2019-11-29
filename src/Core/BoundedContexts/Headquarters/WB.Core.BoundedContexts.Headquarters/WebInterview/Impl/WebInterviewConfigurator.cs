using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview.Impl
{
    internal class WebInterviewConfigurator : IWebInterviewConfigurator
    {
        private readonly IWebInterviewConfigProvider configs;

        public WebInterviewConfigurator(IWebInterviewConfigProvider configs)
        {
            this.configs = configs ?? throw new ArgumentNullException(nameof(configs));
        }

        public void Start(QuestionnaireIdentity questionnaireId, bool useCaptcha)
        {
            var webInterviewConfig = this.configs.Get(questionnaireId);
            webInterviewConfig.Started = true;
            webInterviewConfig.UseCaptcha = useCaptcha;
            this.configs.Store(questionnaireId, webInterviewConfig);
        }

        public void UpdateMessages(QuestionnaireIdentity questionnaireId, Dictionary<WebInterviewUserMessages, string> messages)
        {
            var config = this.configs.Get(questionnaireId);
            config.CustomMessages = messages;
            this.configs.Store(questionnaireId, config);
        }

        public void Stop(QuestionnaireIdentity questionnaireId)
        {
            var config = this.configs.Get(questionnaireId);
            config.Started = false;
            this.configs.Store(questionnaireId, config);
        }

        public void UpdateEmailTextTemplate(QuestionnaireIdentity questionnaireId, EmailTextTemplateType type, EmailTextTemplate emailTextTemplate)
        {
            var config = this.configs.Get(questionnaireId);
            config.EmailTemplates[type] = emailTextTemplate;
            this.configs.Store(questionnaireId, config);
        }
    }
}
