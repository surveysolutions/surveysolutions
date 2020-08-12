using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    public class SharedPerson
    {
        private QuestionnaireListViewItem? questionnaire;
        public int Id { get; set; }

        public virtual string QuestionnaireId { get; set; } = String.Empty;
        public virtual Guid UserId { get; set; }
        public virtual string Email { get; set; } = string.Empty;
        public virtual ShareType ShareType { set; get; }
        public virtual bool IsOwner { get; set; }

        public virtual QuestionnaireListViewItem Questionnaire
        {
            get => questionnaire ?? throw new InvalidOperationException("Cannot return null reference");
            set => questionnaire = value;
        }
    }
}
