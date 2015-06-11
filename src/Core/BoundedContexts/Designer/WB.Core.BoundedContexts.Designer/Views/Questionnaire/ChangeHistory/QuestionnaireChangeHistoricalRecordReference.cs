using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireChangeHistoricalRecordReference
    {
        public QuestionnaireChangeHistoricalRecordReference(
            Guid id, 
            Guid? parentId,
            string title, 
            QuestionnaireItemType type)
        {
            Id = id;
            Title = title;
            Type = type;
            ParentId = parentId;
        }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public Guid? ParentId { get; private set; }
        public QuestionnaireItemType Type { get; private set; }
    }
}