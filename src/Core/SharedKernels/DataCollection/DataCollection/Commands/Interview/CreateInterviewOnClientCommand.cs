using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateInterviewOnClientCommand : InterviewCommand
    {
        public Guid Id { get; }
        public QuestionnaireIdentity QuestionnaireIdentity { get; }
        public Guid SupervisorId { get; }
        public InterviewKey InterviewKey { get; }
        public int? AssignmentId { get; }
        public IReadOnlyList<InterviewAnswer> Answers { get; }
        public DateTime AnswersTime { get; }

        public CreateInterviewOnClientCommand(Guid interviewId, 
            Guid userId, 
            QuestionnaireIdentity questionnaireIdentity, 
            DateTime answersTime, Guid supervisorId, 
            InterviewKey interviewKey, 
            int? assignmentId,
            IReadOnlyList<InterviewAnswer> answersToIdentifyingQuestions)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireIdentity = questionnaireIdentity;
            this.AnswersTime = answersTime;
            this.SupervisorId = supervisorId;
            this.InterviewKey = interviewKey;
            this.AssignmentId = assignmentId;
            this.Answers = answersToIdentifyingQuestions ?? new List<InterviewAnswer>();
        }
    }
}
