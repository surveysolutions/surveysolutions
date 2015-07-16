using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    public class SharedPerson
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public ShareType ShareType { set; get; }
        public bool IsOwner { get; set; }
    }
}