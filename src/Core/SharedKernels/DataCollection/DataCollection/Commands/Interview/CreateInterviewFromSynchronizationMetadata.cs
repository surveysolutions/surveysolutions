using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class CreateInterviewFromSynchronizationMetadata : InterviewCommand
    {
        public CreateInterviewFromSynchronizationMetadata(Guid interviewId, Guid userId, Guid questionnaireId, long questionnaireVersion, InterviewStatus status,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, string comments, DateTime? rejectedDateTime, DateTime? interviewerAssignedDateTime,
            bool valid, bool createdOnClient = false)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.InterviewStatus = status;
            this.FeaturedQuestionsMeta = featuredQuestionsMeta;
            this.Comments = comments;
            this.RejectedDateTime = rejectedDateTime;
            this.Valid = valid;
            this.CreatedOnClient = createdOnClient;
            this.InterviewerAssignedDateTime = interviewerAssignedDateTime;
        }

        public Guid Id { get; set; }

        public Guid QuestionnaireId { get; set; }

        public long QuestionnaireVersion { get; set; }

        public InterviewStatus InterviewStatus { get; set; }

        public AnsweredQuestionSynchronizationDto[] FeaturedQuestionsMeta { get; set; }

        public string Comments { get; set; }

        public bool Valid { get; set; }

        public bool CreatedOnClient { get; set; }

        public DateTime? RejectedDateTime { get; set; }
        public DateTime? InterviewerAssignedDateTime { get; set; }
    }
}
