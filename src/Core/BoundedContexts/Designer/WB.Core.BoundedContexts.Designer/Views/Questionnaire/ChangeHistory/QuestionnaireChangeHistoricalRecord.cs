using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireChangeHistoricalRecord
    {
        public QuestionnaireChangeHistoricalRecord(
            string userName, 
            DateTime timestamp,
            QuestionnaireActionType actionType,
            Guid targetId,
            Guid? targetParentId, 
            string title,
            QuestionnaireItemType type, 
            List<QuestionnaireChangeHistoricalRecordReference> historicalRecordReferences)
        {
            UserName = userName;
            Timestamp = timestamp;
            ActionType = actionType;
            HistoricalRecordReferences = historicalRecordReferences;
            TargetParentId = targetParentId;
            TargetId = targetId;
            TargetTitle = title;
            TargetType = type;
        }

        public string UserName { get; private set; }
        public DateTime Timestamp { get; private set; }
        public QuestionnaireActionType ActionType { get; private set; }

        public Guid TargetId { get; private set; }
        public string TargetTitle { get; private set; }
        public Guid? TargetParentId { get; private set; }
        public QuestionnaireItemType TargetType { get; private set; }

        public List<QuestionnaireChangeHistoricalRecordReference> HistoricalRecordReferences { get; private set; }
    }
}