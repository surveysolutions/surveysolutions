using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate
{
    internal interface IInterviewsToDeleteFactory
    {
        void RemoveAllInterviews(Guid questionnaireId, long questionnaireVersion);
    }
}
