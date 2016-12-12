using WB.Core.SharedKernels.Questionnaire.Documents;

namespace Main.Core.Entities.SubEntities
{
    public interface IConditional : IQuestionnaireEntity
    {
        string ConditionExpression { get; set; }
        bool HideIfDisabled { get; set; }
    }
}
