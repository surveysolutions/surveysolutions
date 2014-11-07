using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireDocumentUpgrader
    {
        QuestionnaireDocument TranslatePropagatePropertiesToRosterProperties(QuestionnaireDocument originalDocument);
    }
}
