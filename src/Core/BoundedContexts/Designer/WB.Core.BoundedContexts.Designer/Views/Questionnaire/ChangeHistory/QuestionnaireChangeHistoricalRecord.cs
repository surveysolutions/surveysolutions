using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireChangeHistoricalRecord
    {
        public QuestionnaireChangeHistoricalRecord(
            string id,
            string? userName,
            DateTime timestamp,
            QuestionnaireActionType actionType,
            Guid targetId,
            Guid? targetParentId,
            string? title,
            QuestionnaireItemType type,
            string? targetNewTitle,
            int? affectedEntries,
            bool hasRevertTo,
            DateTime? targetDateTime,
            List<QuestionnaireChangeHistoricalRecordReference> historicalRecordReferences,
            string? comment, string? hqVersion, long? hqQuestionnaireVersion, bool canEditComment)
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
            this.HasRevertTo = hasRevertTo;
            this.TargetDateTime = targetDateTime;
            this.Comment = comment;
            this.HqVersion = hqVersion;
            this.HqQuestionnaireVersion = hqQuestionnaireVersion;
            this.CanEditComment = canEditComment;
        }

        public string Id { get; }
        public int Sequence { get; set; }
        public string? UserName { get; }
        public DateTime Timestamp { get; private set; }
        public QuestionnaireActionType ActionType { get; }

        public Guid TargetId { get; private set; }
        public string? TargetTitle { get; }
        public Guid? TargetParentId { get; private set; }
        public QuestionnaireItemType TargetType { get; }
        public DateTime? TargetDateTime { get; private set; }
        public string? TargetNewTitle { get; }
        public int? AffectedEntries { get; private set; }

        public bool HasRevertTo { get; }
        public List<QuestionnaireChangeHistoricalRecordReference> HistoricalRecordReferences { get; }
        public string? Comment { get; }
        public bool CanEditComment { get; set; }
        public string? HqUserName { get; set; }
        public string? HqVersion { get; }
        public long? HqQuestionnaireVersion { get; }
    }
}
