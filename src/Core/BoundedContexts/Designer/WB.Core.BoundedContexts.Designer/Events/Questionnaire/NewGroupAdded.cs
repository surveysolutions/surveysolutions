using System;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace Main.Core.Events.Questionnaire
{
    using Ncqrs.Eventing.Storage;

    public class NewGroupAdded : FullGroupDataEvent
    {
        public Guid PublicKey { get; set; }

        public Guid? ParentGroupPublicKey { get; set; }
    }
}