using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireChangeHistoricalRecordReference
    {
        public QuestionnaireChangeHistoricalRecordReference(
            Guid id, 
            Guid? parentId,
            string title, 
            QuestionnaireItemType type, 
            bool isExist)
        {
            Id = id;
            Title = title;
            Type = type;
            IsExist = isExist;
            ParentId = parentId;
        }

        public Guid Id { get; private set; }
        public string Title { get; private set; }
        public Guid? ParentId { get; private set; }
        public bool IsExist { get; private set; }
        public QuestionnaireItemType Type { get; private set; }
    }
}