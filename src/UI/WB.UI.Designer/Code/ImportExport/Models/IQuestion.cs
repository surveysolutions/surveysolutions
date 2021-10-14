#nullable enable
using System;
using System.Collections.Generic;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public interface IQuestion : IQuestionnaireEntity, IConditional, IValidatable
    {
        List<Answer> Answers { get; set; }
        Order? AnswerOrder { get; set; }
        bool Featured { get; set; }
        string? Instructions { get; set; }
        QuestionProperties? Properties { get; set; }
        string? QuestionText { get; set; }
        QuestionType QuestionType { get; set; }
        QuestionScope QuestionScope { get; set; }
        string? VariableLabel { get; set; }
        [Obsolete("Multiple validations should be used instead")]
        string? ValidationExpression { get; set; }
        [Obsolete("Multiple validations should be used instead")]
        string? ValidationMessage { get; set; }
        Guid? LinkedToQuestionId { get; set; }
        Guid? LinkedToRosterId { get; set; }
        string? LinkedFilterExpression { get; set; }
        Guid? CascadeFromQuestionId { get; set; }
        bool? IsFilteredCombobox { get; set; }
        bool IsTimestamp { get; set; }
    }
}
