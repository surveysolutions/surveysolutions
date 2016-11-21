using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    public class SharedPerson
    {
        public virtual Guid Id { get; set; }
        public virtual string Email { get; set; }
        public virtual ShareType ShareType { set; get; }
        public virtual bool IsOwner { get; set; }
    }
}