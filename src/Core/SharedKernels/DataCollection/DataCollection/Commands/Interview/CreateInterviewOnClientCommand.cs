using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateInterviewOnClientCommand : InterviewCommand
    {
        public Guid Id { get; private set; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; private set; }
        public Guid SupervisorId { get; private set; }
        public DateTime AnswersTime { get; private set; }

        public CreateInterviewOnClientCommand(Guid interviewId, Guid userId, QuestionnaireIdentity questionnaireIdentity, 
            DateTime answersTime, Guid supervisorId)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireIdentity = questionnaireIdentity;
            this.AnswersTime = answersTime;
            this.SupervisorId = supervisorId;
        }
    }
}
