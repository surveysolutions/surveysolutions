using System;

namespace WB.Core.BoundedContexts.CAPI.Views.Statistics
{
    public class StatisticsInput
    {
        public StatisticsInput(Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }

        public Guid QuestionnaireId { get; private set; }
    }
}