using System;

namespace WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions
{
    public class QuestionnaireCompilationVersion
    {
        public virtual Guid QuestionnaireId { get; set; }
        public virtual int Version { get; set; }
        public virtual string? Description { get; set; }
    }
}
