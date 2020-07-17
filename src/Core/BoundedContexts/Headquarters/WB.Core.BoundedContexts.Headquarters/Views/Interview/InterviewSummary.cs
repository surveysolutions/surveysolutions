using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewSummary : InterviewBrief
    {
        private string responsibleName;
        private string supervisorName;

        public InterviewSummary()
        {
            this.AnswersToFeaturedQuestions = new List<QuestionAnswer>();
            this.InterviewCommentedStatuses = new List<InterviewCommentedStatus>();
            this.TimeSpansBetweenStatuses = new HashSet<TimeSpanBetweenStatuses>();
            this.Comments = new HashSet<InterviewComment>();
            this.GpsAnswers = new HashSet<InterviewGps>();
        }

        public InterviewSummary(IQuestionnaire questionnaire) : this()
        {
            int position = 0;
            foreach (var featuredQuestionId in questionnaire.GetPrefilledQuestions()
                .Where(x => questionnaire.GetQuestionType(x) != QuestionType.GpsCoordinates))
            {
                var result = new QuestionAnswer
                {
                    Question = new Questionnaire.QuestionnaireCompositeItem
                    {
                        Id = questionnaire.GetEntityIdMapValue(featuredQuestionId)
                    },
                    Answer = string.Empty,
                    InterviewSummary = this,
                    Position = position
                };
                position++;

                this.AnswersToFeaturedQuestions.Add(result);
            }

            this.QuestionnaireVariable = questionnaire.VariableName ?? string.Empty;
        }

        public override Guid InterviewId
        {
            get => base.InterviewId;
            set
            {
                this.SummaryId = value.FormatGuid();
                base.InterviewId = value;
            }
        }

        public virtual string QuestionnaireIdentity { get; set; }

        public virtual string Key { get; set; }

        public virtual int Id { get; set; }
        public virtual string ClientKey { get; set; }

        [PrimaryKeyAlias]
        public virtual string SummaryId { get; set; }
        public virtual string QuestionnaireTitle { get; set; }

        public virtual string ResponsibleName
        {
            get => responsibleName;
            set
            {
                responsibleName = value;
                this.ResponsibleNameLowerCase = value?.ToLower();
            }
        }

        public virtual Guid SupervisorId { get; set; }

        public virtual string SupervisorName
        {
            get => supervisorName;
            set
            {
                supervisorName = value;
                SupervisorNameLowerCase = value?.ToLower();
            }
        }

        public virtual string SupervisorNameLowerCase { get; set; }

        public virtual UserRoles ResponsibleRole { get; set; }
        public virtual DateTime UpdateDate { get; set; }
        public virtual string LastStatusChangeComment { get; set; }

        public virtual bool WasRejectedBySupervisor { get; set; }
        public virtual IList<QuestionAnswer> AnswersToFeaturedQuestions { get; protected set; }

        public virtual bool WasCreatedOnClient { get; set; }
        public virtual DateTime? ReceivedByInterviewerAtUtc { get; set; }
        public virtual bool ReceivedByInterviewer => ReceivedByInterviewerAtUtc.HasValue;
        public virtual bool IsAssignedToInterviewer { get; set; }

        public virtual int? AssignmentId { get; set; }

        public virtual bool WasCompleted { get; set; }

        public virtual DateTime CreatedDate { get; set; }
        public virtual DateTime? FirstAnswerDate { get; set; }
        public virtual Guid? FirstInterviewerId { get; set; }
        public virtual string FirstInterviewerName { get; set; }
        public virtual Guid? FirstSupervisorId { get; set; }
        public virtual string FirstSupervisorName { get; set; }

        public virtual TimeSpan? InterviewDuration
        {
            get => InterviewDurationLong != null ? new TimeSpan(this.InterviewDurationLong.Value) : (TimeSpan?)null;
            set => this.InterviewDurationLong = value?.Ticks;
        }
        public virtual long? InterviewDurationLong { get; protected set; }

        public virtual DateTime? LastResumeEventUtcTimestamp { get; set; }

        public virtual IList<InterviewCommentedStatus> InterviewCommentedStatuses { get; set; }

        public virtual ISet<TimeSpanBetweenStatuses> TimeSpansBetweenStatuses { get; set; }
        public virtual ISet<InterviewGps> GpsAnswers { get; protected set; }
        public virtual ISet<InterviewStatisticsReportRow> StatisticsReport { get; set; } = new HashSet<InterviewStatisticsReportRow>();

        public virtual ISet<InterviewComment> Comments { get; protected set; }

        public virtual int CommentedEntitiesCount { get; set; }

        public virtual bool HasResolvedComments { get; set; }
        public virtual string ResponsibleNameLowerCase { get; protected set; }
        public virtual bool HasSmallSubstitutions { get; set; }

        public virtual void AnswerFeaturedQuestion(int questionId, string answer, decimal? optionCode = null)
        {
            this.AnswersToFeaturedQuestions.First(x => x.Question.Id == questionId).Answer = answer;
            this.AnswersToFeaturedQuestions.First(x => x.Question.Id == questionId).AnswerCode = optionCode;
        }

        public virtual bool CanAnswerFeaturedQuestion(int questionId)
        {
            return this.AnswersToFeaturedQuestions.Any(x => x.Question.Id == questionId);
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
