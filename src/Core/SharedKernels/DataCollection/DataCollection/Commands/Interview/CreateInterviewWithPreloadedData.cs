using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateTemporaryInterviewCommand : InterviewCommand
    {
        public QuestionnaireIdentity QuestionnaireId { get; }

        public CreateTemporaryInterviewCommand(Guid interviewId, Guid userId, QuestionnaireIdentity questionnaireId) : base(interviewId, userId)
        {
            QuestionnaireId = questionnaireId;
        }
    }

    public class CreateInterview : InterviewCommand
    {
        public CreateInterview(Guid interviewId,
            Guid userId,
            QuestionnaireIdentity questionnaireId,
            List<InterviewAnswer> answers,
            List<string> protectedVariables,
            Guid supervisorId,
            Guid? interviewerId,
            InterviewKey interviewKey,
            int? assignmentId)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.Answers = answers;
            this.ProtectedVariables = protectedVariables ?? new List<string>();
            this.SupervisorId = supervisorId;
            this.InterviewerId = interviewerId;
            this.InterviewKey = interviewKey;
            this.AssignmentId = assignmentId;
        }

        public Guid Id { get; private set; }
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public List<InterviewAnswer> Answers { get; private set; }
        public List<string> ProtectedVariables { get; }
        public Guid SupervisorId { get; private set; }
        public Guid? InterviewerId { get; private set; }

        public InterviewKey InterviewKey { get; private set; }
        public int? AssignmentId { get; }
    }
}
