
namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    public interface IConditional : IQuestionnaireEntity
    {
        string ConditionExpression { get; set; }
        bool HideIfDisabled { get; set; }
    }
}
