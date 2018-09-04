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
        public virtual string DiffWithPrevisousVersion { get; set; }
    }
}
