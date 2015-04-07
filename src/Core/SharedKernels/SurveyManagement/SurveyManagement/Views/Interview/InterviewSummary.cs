using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Raven.Client.Linq;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewSummary : InterviewBrief
    {
        private readonly QuestionType[] questionTypesWithOptions = new[] { QuestionType.SingleOption, QuestionType.MultyOption };

        public InterviewSummary()
        {
            this.AnswersToFeaturedQuestions = new HashSet<QuestionAnswer>();
            this.CommentedStatusesHistory = new HashSet<InterviewCommentedStatus>();
            this.QuestionOptions = new HashSet<QuestionOptions>();
        }

        public InterviewSummary(QuestionnaireDocument questionnaire) : this()
        {
            foreach (var featuredQuestion in questionnaire.Find<IQuestion>(q => q.Featured))
            {
                var result = new QuestionAnswer
                {
                    Id = featuredQuestion.PublicKey,
                    Title = featuredQuestion.QuestionText,
                    Answer = string.Empty,
                    Type = featuredQuestion.QuestionType,
                    InterviewSummary = this
                };

                this.AnswersToFeaturedQuestions.Add(result);

                if (featuredQuestion.QuestionType.In(this.questionTypesWithOptions) && featuredQuestion.Answers != null)
                {
                    var options = featuredQuestion.Answers.Select(option => 
                        new QuestionOptions
                        {
                            QuestionId = featuredQuestion.PublicKey,
                            Text = option.AnswerText,
                            Value = decimal.Parse(option.AnswerValue)
                        });
                    foreach (var option in options)
                    {
                        this.QuestionOptions.Add(option);
                    }
                }
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
        public virtual ISet<QuestionAnswer> AnswersToFeaturedQuestions { get; protected set; }
        public virtual ISet<InterviewCommentedStatus> CommentedStatusesHistory { get; protected set; }
        public virtual ISet<QuestionOptions> QuestionOptions { get; protected set; }

        public virtual bool WasCreatedOnClient { get; set; }

        public virtual void AnswerFeaturedQuestion(Guid questionId, string answer)
        {
            this.AnswersToFeaturedQuestions.First(x => x.Id == questionId).Answer = answer;
        }

        public virtual void AnswerFeaturedQuestion(Guid questionId, decimal[] answers)
        {
            var questionOptions = this.QuestionOptions.Where(x => x.QuestionId == questionId);

            var optionStrings =  answers.Select(answerValue => questionOptions.First(x => x.Value == answerValue).Text)
                                       .ToList();

            this.AnswerFeaturedQuestion(questionId, string.Join(",", optionStrings));
        }
    }

    public class InterviewCommentedStatus
    {
        public virtual int Id { get; set; }
        public virtual string Comment { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual InterviewStatus Status { get; set; }
        public virtual string Responsible { get; set; }
        public virtual Guid ResponsibleId { get; set; }
    }

    public class QuestionAnswer
    {
        public virtual Guid Id { get; set; }
        public virtual string Title { get; set; }
        public virtual string Answer { get; set; }
        public QuestionType Type { get; set; }

        public virtual InterviewSummary InterviewSummary { get; set; }

        protected bool Equals(QuestionAnswer other)
        {
            return Equals(this.InterviewSummary, other.InterviewSummary) && this.Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QuestionAnswer)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.InterviewSummary != null ? this.InterviewSummary.GetHashCode() : 0) * 397) ^ this.Id.GetHashCode();
            }
        }
    }

    public class QuestionOptions
    {
        public virtual int Id { get; set; }
        public virtual Guid QuestionId { get; set; }
        public virtual decimal Value { get; set; }
        public virtual string Text { get; set; }
    }
}