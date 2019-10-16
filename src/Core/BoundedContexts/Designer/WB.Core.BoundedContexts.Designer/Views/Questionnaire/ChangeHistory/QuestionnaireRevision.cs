using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireChangeRecord
    {
        public virtual string QuestionnaireChangeRecordId { get; set; }
        public virtual string QuestionnaireId { get; set; }
        public virtual Guid UserId { get; set; }
        public virtual string UserName { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual int Sequence { get; set; }
        public virtual QuestionnaireActionType ActionType { get; set; }
        public virtual QuestionnaireItemType TargetItemType { get; set; }
        public virtual Guid TargetItemId { get; set; }
        public virtual string TargetItemTitle { get; set; }
        public virtual string TargetItemNewTitle { get; set; }
        public virtual int? AffectedEntriesCount { get; set; }
        public virtual ISet<QuestionnaireChangeReference> References { get; set; } = new HashSet<QuestionnaireChangeReference>();
        public virtual string ResultingQuestionnaireDocument { get; set; }
        public virtual DateTime? TargetItemDateTime { get; set; }
        public virtual string Patch { get; set; }
        
        public virtual QuestionnaireChangeRecordMetadata Meta {get;set;}        
    }
  
    public class QuestionnaireRevision
    {
        public QuestionnaireRevision(Guid questionnaireId, Guid? revision = null)
        {
            this.QuestionnaireId = questionnaireId;
            this.Revision = revision;
        }

        public QuestionnaireRevision(string questionnaireId)
        {
            this.QuestionnaireId = Guid.Parse(questionnaireId);
        }

        public Guid QuestionnaireId { get; }
        public Guid? Revision { get; }

        public override string ToString() => Revision.HasValue ? $"{QuestionnaireId:N}${Revision:N}" : $"{QuestionnaireId:N}";
    }

    public class QuestionnaireChangeRecordMetadata
    {
        public string HqHostName { get; set; }
        public string HqUserName { get; set; }
        public string HqVersion { get; set; }
        public string HqBuild { get; set; }
        public string Comment { get; set; }
        public float HqTimeZoneMinutesOffset { get; set; }
        public string HqImporterLogin { get; set; }
        public long QuestionnaireVersion { get; set; }
    }
}
