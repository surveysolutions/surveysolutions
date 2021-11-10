using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    [DebuggerDisplay("StaticText {PublicKey}")]
    public class StaticText : QuestionnaireEntity
    {
        public string Text { get; set; } = String.Empty;

        public string? AttachmentName { get; set; }
        public List<ValidationCondition>? ValidationConditions { get; set; }
        public string? ConditionExpression { get; set; }
        public bool HideIfDisabled { get; set; }
    }
}
