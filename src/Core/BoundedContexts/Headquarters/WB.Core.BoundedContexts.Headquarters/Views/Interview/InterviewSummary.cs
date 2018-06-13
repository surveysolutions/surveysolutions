using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewSummary : InterviewBrief
    {
        public InterviewSummary()
        {
            this.AnswersToFeaturedQuestions = new List<QuestionAnswer>();
            this.InterviewCommentedStatuses = new List<InterviewCommentedStatus>();
            this.TimeSpansBetweenStatuses = new HashSet<TimeSpanBetweenStatuses>();
        }

        public InterviewSummary(QuestionnaireDocument questionnaire) : this()
        {
            int position = 0;
            foreach (var featuredQuestion in questionnaire.Find<IQuestion>(q => q.Featured && q.QuestionType != QuestionType.GpsCoordinates))
            {
                var result = new QuestionAnswer
                {
                    Questionid = featuredQuestion.PublicKey,
                    Title = featuredQuestion.QuestionText,
                    Answer = string.Empty,
                    Type = featuredQuestion.QuestionType,
                    InterviewSummary = this,
                    Position = position
                };
                position++;

                this.AnswersToFeaturedQuestions.Add(result);
            }
        }

        public override Guid InterviewId
        {
            get { return base.InterviewId; }
            set
            {
                this.SummaryId = value.FormatGuid();
                base.InterviewId = value;
            }
        }

        public virtual string QuestionnaireIdentity { get; set; }

        public virtual string Key { get; set; }

        public virtual string ClientKey { get; set; }
        
        public virtual string SummaryId { get; set; }
        public virtual string QuestionnaireTitle { get; set; }
        public virtual string ResponsibleName { get; set; }
        public virtual Guid TeamLeadId { get; set; }
        public virtual string TeamLeadName { get; set; }
        public virtual UserRoles ResponsibleRole { get; set; }
        public virtual DateTime UpdateDate { get; set; }
        public virtual string LastStatusChangeComment { get; set; }

        public virtual bool WasRejectedBySupervisor { get; set; }
        public virtual IList<QuestionAnswer> AnswersToFeaturedQuestions { get; protected set; }

        public virtual bool WasCreatedOnClient { get; set; }
        public virtual bool ReceivedByInterviewer { get; set; }
        public virtual bool IsAssignedToInterviewer { get; set; }

        public virtual int? AssignmentId { get; set; }

        public virtual bool WasCompleted { get; set; }

        public virtual TimeSpan? InterviewDuration
        {
            get => InterviewDurationLong != null ? new TimeSpan(this.InterviewDurationLong.Value) : (TimeSpan?)null;
            set => this.InterviewDurationLong = value?.Ticks;
        }
        public virtual long? InterviewDurationLong { get; protected set; }
        
        public virtual DateTime? LastResumeEventUtcTimestamp { get; set; }

        public virtual IList<InterviewCommentedStatus> InterviewCommentedStatuses { get; set; }

        public virtual ISet<TimeSpanBetweenStatuses> TimeSpansBetweenStatuses { get; set; }

        public virtual int CommentedEntitiesCount { get; set; }
        
        public virtual void AnswerFeaturedQuestion(Guid questionId, string answer)
        {
            this.AnswersToFeaturedQuestions.First(x => x.Questionid == questionId).Answer = answer;
        }

        protected bool Equals(InterviewSummary other)
        {
            return string.Equals(this.SummaryId, other.SummaryId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return this.Equals((InterviewSummary)obj);
        }

        public override int GetHashCode()
        {
            return SummaryId?.GetHashCode() ?? 0;
        }
    }
}
