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

        public Order? AnswerOrder { get; set; }

        public List<Answer> Answers { get; set; } = new List<Answer>();

        public string? Comments { get; set; }

        public string? ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public bool Featured { get; set; }

        public string? Instructions { get; set; }

        public QuestionProperties? Properties { get; set; }

        public QuestionScope QuestionScope { get; set; }

        public string? QuestionText { get; set; }

        public virtual QuestionType QuestionType { get; set; }

        public string? VariableLabel { get; set; }

        public string? ValidationExpression { get; set; }

        public string? ValidationMessage { get; set; }

        public Guid? LinkedToRosterId { get; set; }

        public Guid? CascadeFromQuestionId { get; set; }

        public Guid? LinkedToQuestionId { get; set; }

        public string? LinkedFilterExpression { get; set; }

        public bool? IsFilteredCombobox { get; set; }

        public bool IsTimestamp { get; set; }

        public IList<ValidationCondition>? ValidationConditions { get; set; } 
    }
}
