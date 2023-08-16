using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class SharedPersonView
    {
        public Guid UserId { get; set; }
        public string? Email { get; set; } = String.Empty;
        public string? Login { get; set; } = String.Empty;
        public ShareType ShareType { set; get; }
        public bool IsOwner { get; set; }
    }
}
