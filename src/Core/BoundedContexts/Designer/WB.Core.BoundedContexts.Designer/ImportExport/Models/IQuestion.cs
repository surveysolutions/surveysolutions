#nullable enable
namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    public interface IQuestion : IQuestionnaireEntity, IConditional, IValidatable
    {
        string? VariableName { get; set; } 
        string? Instructions { get; set; }
        bool HideInstructions { get; set; }
        string? QuestionText { get; set; }
        QuestionScope QuestionScope { get; set; }
        string? VariableLabel { get; set; }
    }
}
