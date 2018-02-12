using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public interface IWebInterviewConfigurator
    {
        void Start(QuestionnaireIdentity questionnaireId, bool useCaptcha,
            Dictionary<WebInterviewUserMessages, string> customMessages);
        void Stop(QuestionnaireIdentity questionnaireId);
    }
}