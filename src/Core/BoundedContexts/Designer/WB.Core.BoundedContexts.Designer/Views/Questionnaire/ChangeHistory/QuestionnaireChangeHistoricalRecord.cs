using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireChangeHistoricalRecord
    {
        public QuestionnaireChangeHistoricalRecord(
            string id,
            string userName, 
            DateTime timestamp,
            QuestionnaireActionType actionType,
            Guid targetId,
            Guid? targetParentId, 
            string title,
            QuestionnaireItemType type, 
            string targetNewTitle,
            int? affectedEntries,
            List<QuestionnaireChangeHistoricalRecordReference> historicalRecordReferences)
        {
            this.Id = id;
            UserName = userName;
            Timestamp = timestamp;
            ActionType = actionType;
            HistoricalRecordReferences = historicalRecordReferences;
            TargetParentId = targetParentId;
            TargetId = targetId;
            TargetTitle = title;
            TargetType = type;
            this.TargetNewTitle = targetNewTitle;
            this.AffectedEntries = affectedEntries;
        }

        public string Id { get; private set; }

        public string UserName { get; private set; }
        public DateTime Timestamp { get; private set; }
        public QuestionnaireActionType ActionType { get; private set; }

        public Guid TargetId { get; private set; }
        public string TargetTitle { get; private set; }
        public Guid? TargetParentId { get; private set; }
        public QuestionnaireItemType TargetType { get; private set; }
        public string TargetNewTitle { get; private set; }
        public int? AffectedEntries { get; private set; }

        public List<QuestionnaireChangeHistoricalRecordReference> HistoricalRecordReferences { get; private set; }
    }
}