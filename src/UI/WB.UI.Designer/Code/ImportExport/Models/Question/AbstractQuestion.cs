using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace WB.UI.Designer.Code.ImportExport.Models.Question
{
    [DebuggerDisplay("{GetType().Name} {VariableName} {PublicKey}")]
    public abstract class AbstractQuestion : IQuestion
    {
        public Order? AnswerOrder { get; set; }

        public List<Answer> Answers { get; set; } = new List<Answer>();

        public ReadOnlyCollection<IQuestionnaireEntity>? Children { get; set; }

        public string? Comments { get; set; }

        public string? ConditionExpression { get; set; }

        public bool HideIfDisabled { get; set; }

        public bool Featured { get; set; }

        public string? Instructions { get; set; }

        public QuestionProperties? Properties { get; set; }

        public Guid PublicKey { get; set; }

        public QuestionScope QuestionScope { get; set; }

        public string? QuestionText { get; set; }

        public virtual QuestionType QuestionType { get; set; }

        public string? VariableName { get; set; }

        public string? VariableLabel { get; set; }

        public string? ValidationExpression { get; set; }

        public string? ValidationMessage { get; set; }

        public Guid? LinkedToRosterId { get; set; }

        /// <summary>
        /// Id of parent question to cascade from 
        /// </summary>
        public Guid? CascadeFromQuestionId { get; set; }

        public Guid? LinkedToQuestionId { get; set; }

        public string? LinkedFilterExpression { get; set; }

        public bool? IsFilteredCombobox { get; set; }

        public bool IsTimestamp { get; set; }

        public IList<ValidationCondition>? ValidationConditions { get; set; } 
    }
}
