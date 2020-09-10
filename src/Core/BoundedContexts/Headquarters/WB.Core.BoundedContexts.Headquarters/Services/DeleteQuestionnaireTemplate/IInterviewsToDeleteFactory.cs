using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate
{
    internal interface IInterviewsToDeleteFactory
    {
        void RemoveAllInterviews(QuestionnaireIdentity questionnaireIdentity);
        void RemoveAllEventsForInterviews(QuestionnaireIdentity questionnaireIdentity);
        void RemoveAudioAuditForInterviews(QuestionnaireIdentity questionnaireIdentity);
        void RemoveAudioForInterviews(QuestionnaireIdentity questionnaireIdentity);
    }
}
