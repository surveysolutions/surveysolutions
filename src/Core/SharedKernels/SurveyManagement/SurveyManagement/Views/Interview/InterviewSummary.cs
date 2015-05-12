using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewSummary : InterviewBrief
    {
        public InterviewSummary()
        {
            this.AnswersToFeaturedQuestions = new HashSet<QuestionAnswer>();
        }

        public InterviewSummary(QuestionnaireDocument questionnaire) : this()
        {
            foreach (var featuredQuestion in questionnaire.Find<IQuestion>(q => q.Featured))
            {
                var result = new QuestionAnswer
                {
                    Questionid = featuredQuestion.PublicKey,
                    Title = featuredQuestion.QuestionText,
                    Answer = string.Empty,
                    Type = featuredQuestion.QuestionType,
                    InterviewSummary = this
                };

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

        public virtual string SummaryId { get; set; }
        public virtual string QuestionnaireTitle { get; set; }
        public virtual string ResponsibleName { get; set; }
        public virtual Guid TeamLeadId { get; set; }
        public virtual string TeamLeadName { get; set; }
        public virtual UserRoles ResponsibleRole { get; set; }
        public virtual DateTime UpdateDate { get; set; }
        public virtual string LastStatusChangeComment { get; set; }

        public virtual bool WasRejectedBySupervisor { get; set; }
        public virtual ISet<QuestionAnswer> AnswersToFeaturedQuestions { get; protected set; }

        public virtual bool WasCreatedOnClient { get; set; }

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
            return Equals((InterviewSummary)obj);
        }

        public override int GetHashCode()
        {
            return (this.SummaryId != null ? this.SummaryId.GetHashCode() : 0);
        }
    }
}