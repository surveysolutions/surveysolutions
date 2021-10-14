
namespace WB.UI.Designer.Code.ImportExport.Models
{
    public interface IConditional : IQuestionnaireEntity
    {
        string ConditionExpression { get; set; }
        bool HideIfDisabled { get; set; }
    }
}
