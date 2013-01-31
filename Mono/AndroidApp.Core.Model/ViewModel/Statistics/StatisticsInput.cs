using System;

namespace AndroidApp.Core.Model.ViewModel.Statistics
{
    public class StatisticsInput
    {
        public StatisticsInput(Guid questionnaireId)
        {
            QuestionnaireId = questionnaireId;
        }

        public Guid QuestionnaireId { get; private set; }
    }
}