using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using NJsonSchema.Annotations;


namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    [DebuggerDisplay("{GetType().Name} {VariableName} {PublicKey}")]
    public abstract class AbstractQuestion : QuestionnaireEntity, IQuestion
    {
        public string? VariableName { get; set; } 

        public string? Comments { get; set; }

        public string? ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public string? Instructions { get; set; }

        public bool? HideInstructions { get; set; }

        public QuestionScope QuestionScope { get; set; }

        public string? QuestionText { get; set; }

        // public virtual QuestionType QuestionType { get; set; }

        public string? VariableLabel { get; set; }

        public IList<ValidationCondition>? ValidationConditions { get; set; } 
    }
}
