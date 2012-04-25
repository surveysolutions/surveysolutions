using System;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteGroupHeaders
    {
        public Guid PublicKey { get; set; }

        public string GroupText { get; set; }

        public bool IsCurrent { get; set; }

        public Counter Totals { get; set; }

        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, PublicKey);
        }
    }
}