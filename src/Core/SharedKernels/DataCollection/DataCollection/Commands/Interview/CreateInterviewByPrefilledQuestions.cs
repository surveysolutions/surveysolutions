using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateInterviewByPrefilledQuestions : InterviewCommand
    {
        public CreateInterviewByPrefilledQuestions(Guid interviewId, Guid responsibleId,
            QuestionnaireIdentity questionnaireIdentity, DateTime answersTime, Guid supervisorId, Guid? interviewerId,
            Dictionary<Guid, object> answersOnPrefilledQuestions)
            : base(interviewId, responsibleId)
        {
            this.AnswersTime = answersTime;
            this.SupervisorId = supervisorId;
            this.InterviewerId = interviewerId;
            this.AnswersOnPrefilledQuestions = answersOnPrefilledQuestions;
            this.QuestionnaireIdentity = questionnaireIdentity;
        }

        public readonly QuestionnaireIdentity QuestionnaireIdentity;
        public readonly Dictionary<Guid, object> AnswersOnPrefilledQuestions;
        public readonly DateTime AnswersTime;
        public readonly Guid SupervisorId;
        public readonly Guid? InterviewerId;
    }
}
