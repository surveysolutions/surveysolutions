using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireChangeHistoricalRecordReference
    {
        public QuestionnaireChangeHistoricalRecordReference(
            Guid id,
            string title, 
            QuestionnaireItemType type)
        {
            Id = id;
            Title = title;
            Type = type;
        }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public QuestionnaireItemType Type { get; private set; }
    }
}