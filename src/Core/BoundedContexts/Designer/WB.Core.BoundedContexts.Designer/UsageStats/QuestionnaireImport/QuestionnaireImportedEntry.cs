using System;

namespace WB.Core.BoundedContexts.Designer.UsageStats.QuestionnaireImport
{
    public class QuestionnaireImportedEntry
    {
        public virtual DateTime ImportDateUtc { get; set; }

        public virtual Guid QuestionnaireId { get; set; }

        public virtual int[] SupportedByHqVersion { get; set; }
    }
}