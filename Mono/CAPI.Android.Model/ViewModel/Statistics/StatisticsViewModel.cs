using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace CAPI.Android.Core.Model.ViewModel.Statistics
{
    public class StatisticsViewModel
    {
        public StatisticsViewModel(Guid questionnaireId, string title, SurveyStatus status, int totalQuestionCount, IList<StatisticsQuestionViewModel> unansweredQuestions, IList<StatisticsQuestionViewModel> answeredQuestions, IList<StatisticsQuestionViewModel> invalidQuestions)
        {
            QuestionnaireId = questionnaireId;
            Title = title;
            Status = status;
            TotalQuestionCount = totalQuestionCount;
            UnansweredQuestions = unansweredQuestions;
            AnsweredQuestions = answeredQuestions;
            InvalidQuestions = invalidQuestions;
        }

        public Guid QuestionnaireId { get; private set; }
        public string Title { get; private set; }
        public SurveyStatus Status { get; private set; }
        public int TotalQuestionCount { get; set; }

        public IList<StatisticsQuestionViewModel> UnansweredQuestions { get; private set; }
        public IList<StatisticsQuestionViewModel> AnsweredQuestions { get; private set; }
        public IList<StatisticsQuestionViewModel> InvalidQuestions { get; private set; }
    }
}