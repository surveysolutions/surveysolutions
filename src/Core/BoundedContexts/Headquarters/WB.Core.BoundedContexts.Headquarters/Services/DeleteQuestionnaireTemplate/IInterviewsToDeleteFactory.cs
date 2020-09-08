using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate
{
    internal interface IInterviewsToDeleteFactory
    {
        List<Guid> LoadBatch(Guid questionnaireId, long questionnaireVersion);
    }
}
