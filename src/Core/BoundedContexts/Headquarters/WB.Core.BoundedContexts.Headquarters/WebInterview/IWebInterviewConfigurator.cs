using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public interface IWebInterviewConfigurator
    {
        void Start(QuestionnaireIdentity questionnaireId, bool useCaptcha);

        void UpdateMessages(QuestionnaireIdentity questionnaireId,
            Dictionary<WebInterviewUserMessages, string> messages);
        void Stop(QuestionnaireIdentity questionnaireId);
    }
}
