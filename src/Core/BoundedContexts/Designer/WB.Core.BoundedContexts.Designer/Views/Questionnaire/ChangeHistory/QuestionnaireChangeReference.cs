using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory
{
    public class QuestionnaireChangeReference
    {
        public virtual int Id { get; set; }
        public virtual QuestionnaireItemType ReferenceType { get; set; }
        public virtual Guid ReferenceId { get; set; }
        public virtual string ReferenceTitle { get; set; }
        public virtual QuestionnaireChangeRecord QuestionnaireChangeRecord { get; set; }
    }
}