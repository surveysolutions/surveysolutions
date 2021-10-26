#nullable enable
using System;
using System.Collections.Generic;

namespace WB.UI.Designer.Code.ImportExport.Models
{
    public interface IQuestion : IQuestionnaireEntity, IConditional, IValidatable
    {
        string? Instructions { get; set; }
        bool? HideInstructions { get; set; }
        string? QuestionText { get; set; }
        //QuestionType QuestionType { get; set; }
        QuestionScope QuestionScope { get; set; }
        string? VariableLabel { get; set; }
    }
}
