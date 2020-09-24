using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate
{
    internal interface IInterviewsToDeleteFactory
    {
        Task RemoveAllInterviewsDataAsync(QuestionnaireIdentity questionnaireIdentity);
    }
}
