using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.Statistics
{
    public class StatisticsViewModel
    {
        public StatisticsViewModel(Guid questionnaireId, string title, InterviewStatus status, int totalQuestionCount,
                                   IList<StatisticsQuestionViewModel> unansweredQuestions,
                                   IList<StatisticsQuestionViewModel> answeredQuestions,
                                   IList<StatisticsQuestionViewModel> invalidQuestions)
        {
            this.QuestionnaireId = questionnaireId;
            this.Title = title;
            this.Status = status;
            this.TotalQuestionCount = totalQuestionCount;
            this.UnansweredQuestions = unansweredQuestions;
            this.AnsweredQuestions = answeredQuestions;
            this.InvalidQuestions = invalidQuestions;
        }

        public Guid QuestionnaireId { get; private set; }
        public string Title { get; private set; }
        public InterviewStatus Status { get; private set; }
        public int TotalQuestionCount { get; set; }

        public IList<StatisticsQuestionViewModel> UnansweredQuestions { get; private set; }
        public IList<StatisticsQuestionViewModel> AnsweredQuestions { get; private set; }
        public IList<StatisticsQuestionViewModel> InvalidQuestions { get; private set; }
    }
}