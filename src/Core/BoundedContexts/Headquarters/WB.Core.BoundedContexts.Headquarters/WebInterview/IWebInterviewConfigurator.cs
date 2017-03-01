using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public interface IWebInterviewConfigurator
    {
        void Start(QuestionnaireIdentity questionnaireId, Guid responsible, bool useCaptcha);
        void Stop(QuestionnaireIdentity questionnaireId);
    }
}