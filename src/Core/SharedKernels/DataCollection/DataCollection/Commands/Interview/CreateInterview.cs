using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
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
            int? assignmentId,
            bool? isAudioRecordingEnabled,
            InterviewMode mode)
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
            this.IsAudioRecordingEnabled = isAudioRecordingEnabled;
            this.Mode = mode;
        }

        public Guid Id { get; }
        public QuestionnaireIdentity QuestionnaireId { get; set; }
        public List<InterviewAnswer> Answers { get; }
        public List<string> ProtectedVariables { get; }
        public Guid SupervisorId { get; }
        public Guid? InterviewerId { get; }

        public InterviewKey InterviewKey { get; }
        public int? AssignmentId { get; }

        public bool? IsAudioRecordingEnabled { get; }
        public InterviewMode Mode { get; }
    }
}
