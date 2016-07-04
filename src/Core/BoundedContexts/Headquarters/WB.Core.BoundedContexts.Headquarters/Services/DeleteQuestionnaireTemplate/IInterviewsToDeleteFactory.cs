using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate
{
    internal interface IInterviewsToDeleteFactory
    {
        List<InterviewSummary> Load(Guid questionnaireId,
            long questionnaireVersion);
    }
}
