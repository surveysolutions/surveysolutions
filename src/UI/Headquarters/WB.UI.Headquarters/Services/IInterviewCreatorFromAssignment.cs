
using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.Headquarters.Services
{
    public interface IInterviewCreatorFromAssignment
    {
        void CreateInterviewIfQuestionnaireIsOld(HqUser responsible, QuestionnaireIdentity questionnaireIdentity, int assignmentId, IList<IdentifyingAnswer> identifyingData);
        void CreateInterviewIfQuestionnaireIsOld(Guid responsibleId, QuestionnaireIdentity questionnaireIdentity, int assignmentId, List<InterviewAnswer> interviewAnswers);
    }
}