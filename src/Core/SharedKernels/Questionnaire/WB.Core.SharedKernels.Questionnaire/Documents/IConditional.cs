using WB.Core.SharedKernels.Questionnaire.Documents;

namespace Main.Core.Entities.SubEntities
{
    /// <summary>
    /// The Conditional interface.
    /// </summary>
    public interface IConditional : IQuestionnaireEntity
    {
        /// <summary>
        /// Gets or sets the condition expression.
        /// </summary>
        string ConditionExpression { get; set; }
        bool HideIfDisabled { get; set; }
    }
}
