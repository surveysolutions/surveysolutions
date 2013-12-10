using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
{
    public class InterviewSummary : InterviewBrief
    {
        private readonly QuestionType[] questionTypesWithOptions = new[] { QuestionType.SingleOption, QuestionType.MultyOption };

        public InterviewSummary()
        {
            AnswersToFeaturedQuestions = new Dictionary<Guid, QuestionAnswer>();
            CommentedStatusesHistory = new List<InterviewCommentedStatus>();
        }

        public InterviewSummary(QuestionnaireDocument questionnaire)
        {
            CommentedStatusesHistory = new List<InterviewCommentedStatus>();
            AnswersToFeaturedQuestions = new Dictionary<Guid, QuestionAnswer>();

            foreach (var featuredQuestion in questionnaire.Find<IQuestion>(q => q.Featured))
            {
                AnswersToFeaturedQuestions[featuredQuestion.PublicKey] = questionTypesWithOptions.Contains(featuredQuestion.QuestionType)
                    ? CreateFeaturedQuestionWithOptions(featuredQuestion)
                    : CreateFeaturedQuestion(featuredQuestion);
            }
        }

        private QuestionAnswer CreateFeaturedQuestion(IQuestion featuredQuestion)
        {
            return new QuestionAnswer
            {
                Id = featuredQuestion.PublicKey,
                Title = featuredQuestion.QuestionText,
                Answer = string.Empty
            };
        }

        private QuestionAnswerWithOptions CreateFeaturedQuestionWithOptions(IQuestion featuredQuestion)
        {
            return new QuestionAnswerWithOptions()
            {
                Id = featuredQuestion.PublicKey,
                Title = featuredQuestion.QuestionText,
                Options = featuredQuestion.Answers.Select(option=> new QuestionOptions(){Text = option.AnswerText,Value = decimal.Parse(option.AnswerValue)}).ToList(),
                Answer = string.Empty
            };
        }

        public string QuestionnaireTitle { get; set; }
        public string ResponsibleName { get; set; }
        public Guid TeamLeadId { get; set; }
        public string TeamLeadName { get; set; }
        public UserRoles ResponsibleRole { get; set; }
        public DateTime UpdateDate { get; set; }
        public Dictionary<Guid, QuestionAnswer> AnswersToFeaturedQuestions { get; set; }
        public List<InterviewCommentedStatus> CommentedStatusesHistory { get; set; }
    }

    public class InterviewCommentedStatus
    {
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public InterviewStatus Status { get; set; }
    }

    public class QuestionAnswer
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Answer { get; set; }
    }

    public class QuestionAnswerWithOptions : QuestionAnswer
    {
        public List<QuestionOptions> Options { get; set; }

        public void SetAnswerAsAnswerValues(decimal[] answerValues)
        {
            var options = Options.Where(o => answerValues.Contains(o.Value)).Select(o=>o.Text);

            Answer = string.Join(",", options);
        }
    }

    public class QuestionOptions
    {
        public decimal Value { get; set; }
        public string Text { get; set; }
    }
}