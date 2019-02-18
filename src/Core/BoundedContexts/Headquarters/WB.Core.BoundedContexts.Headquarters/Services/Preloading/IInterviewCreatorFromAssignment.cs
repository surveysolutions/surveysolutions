using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services.Preloading
{
    public interface IInterviewCreatorFromAssignment
    {
        void CreateInterviewIfQuestionnaireIsOld(Guid responsibleId, QuestionnaireIdentity questionnaireIdentity, int assignmentId, List<InterviewAnswer> interviewAnswers);
    }
}
