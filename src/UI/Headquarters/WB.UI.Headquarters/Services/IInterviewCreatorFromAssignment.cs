using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code.CommandTransformation;

namespace WB.UI.Headquarters.Services
{
    public interface IInterviewCreatorFromAssignment
    {
        void CreateInterviewIfQuestionnaireIsOld(HqUser responsible, QuestionnaireIdentity questionnaireIdentity, int assignmentId, List<AssignmentIdentifyingDataItem> identifyingData);
        void CreateInterviewIfQuestionnaireIsOld(Guid responsibleId, QuestionnaireIdentity questionnaireIdentity, int assignmentId, List<InterviewAnswer> interviewAnswers);
    }
}