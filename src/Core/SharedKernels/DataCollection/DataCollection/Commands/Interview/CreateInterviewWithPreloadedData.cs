using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateInterviewWithPreloadedData : InterviewCommand
    {
        public CreateInterviewWithPreloadedData(Guid interviewId, 
            Guid userId, 
            Guid questionnaireId, 
            long version,
            List<InterviewAnswer> answers, 
            DateTime answersTime, 
            Guid supervisorId, 
            Guid? interviewerId, 
            InterviewKey interviewKey,
            int? assignmentId)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Answers = answers;
            this.AnswersTime = answersTime;
            this.SupervisorId = supervisorId;
            this.InterviewerId = interviewerId;
            this.InterviewKey = interviewKey;
            this.AssignmentId = assignmentId;
        }

        public Guid Id { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public long Version { get; private set; }
        public List<InterviewAnswer> Answers { get; private set; }
        public Guid SupervisorId { get; private set; }
        public Guid? InterviewerId { get; private set; }

        public InterviewKey InterviewKey { get; private set; }
        public int? AssignmentId { get; }
        public DateTime AnswersTime { get; private set; }
    }
}
