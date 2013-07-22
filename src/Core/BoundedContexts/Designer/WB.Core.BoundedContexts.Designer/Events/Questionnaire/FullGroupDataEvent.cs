using System;
using Main.Core.Entities.SubEntities;

namespace Main.Core.Events.Questionnaire
{
    public class FullGroupDataEvent
    {
        public string ConditionExpression { get; set; }
        public string GroupText { get; set; }
        public Guid? ParentGroupPublicKey { get; set; }
        public Propagate Paropagateble { get; set; }
        public Guid PublicKey { get; set; }
        public string Description { get; set; }
    }
}