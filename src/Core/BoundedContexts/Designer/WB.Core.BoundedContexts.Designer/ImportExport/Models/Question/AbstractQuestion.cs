using System.Collections.Generic;
using System.Diagnostics;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models.Question
{
    [DebuggerDisplay("{GetType().Name} {VariableName} {PublicKey}")]
    public abstract class AbstractQuestion : QuestionnaireEntity, IQuestion
    {
        public string? VariableName { get; set; } 

        public string? Comments { get; set; }

        public string? ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public string? Instructions { get; set; }

        public bool HideInstructions { get; set; }

        public QuestionScope QuestionScope { get; set; }

        public string? QuestionText { get; set; }

        public string? VariableLabel { get; set; }

        public IList<ValidationCondition>? ValidationConditions { get; set; } 
    }
}
