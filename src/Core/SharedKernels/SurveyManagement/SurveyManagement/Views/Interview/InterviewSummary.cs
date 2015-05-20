using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewSummary : InterviewBrief
    {
        private readonly QuestionType[] questionTypesWithOptions = new[] { QuestionType.SingleOption, QuestionType.MultyOption };

        public InterviewSummary()
        {
            this.AnswersToFeaturedQuestions = new HashSet<QuestionAnswer>();
            this.QuestionOptions = new HashSet<QuestionOptions>();
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

                if (this.questionTypesWithOptions.Contains(featuredQuestion.QuestionType) && featuredQuestion.Answers != null)
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
        public virtual string LastStatusChangeComment { get; set; }

        public virtual bool WasRejectedBySupervisor { get; set; }
        public virtual ISet<QuestionAnswer> AnswersToFeaturedQuestions { get; protected set; }
        public virtual ISet<QuestionOptions> QuestionOptions { get; protected set; }

        public virtual bool WasCreatedOnClient { get; set; }

        public virtual void AnswerFeaturedQuestion(Guid questionId, string answer)
        {
            this.AnswersToFeaturedQuestions.First(x => x.Questionid == questionId).Answer = answer;
        }

        public virtual void AnswerFeaturedQuestion(Guid questionId, decimal[] answers)
        {
            var questionOptions = this.QuestionOptions.Where(x => x.QuestionId == questionId);

            var optionStrings =  answers.Select(answerValue => questionOptions.First(x => x.Value == answerValue).Text)
                                       .ToList();

            this.AnswerFeaturedQuestion(questionId, string.Join(",", optionStrings));
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