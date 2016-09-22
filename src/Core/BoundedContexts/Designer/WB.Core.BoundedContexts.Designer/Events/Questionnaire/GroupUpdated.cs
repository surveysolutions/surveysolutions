using System;

namespace WB.Core.BoundedContexts.Designer.Events.Questionnaire
{
    public class GroupUpdated : FullGroupDataEvent
    {
        public Guid GroupPublicKey { get; set; }
    }
}