using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    public class SharedPerson
    {
        public int Id { get; set; }

        public virtual string QuestionnaireId { get; set; }
        public virtual string UserId { get; set; }
        public virtual string Email { get; set; }
        public virtual ShareType ShareType { set; get; }
        public virtual bool IsOwner { get; set; }

        public virtual QuestionnaireListViewItem Questionnaire { get; set; }
    }
}
