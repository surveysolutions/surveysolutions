using System;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteGroupHeaders
    {

        public CompleteGroupHeaders(ICompleteGroup group)
        {
            PublicKey = group.PublicKey;
            GroupText = group.Title;
            PropagationKey = group.PropogationPublicKey;
        }

        public CompleteGroupHeaders()
        {
        }

        public bool Enabled { get; set; }

        public Guid PublicKey { get; set; }

        public string GroupText { get; set; }

        public bool IsCurrent { get; set; }

        public Guid? PropagationKey { get; set; }

        public Counter Totals { get; set; }

        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, PublicKey);
        }
    }
}