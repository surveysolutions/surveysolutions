using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IMacrosSubstitutionService
    {
        string SubstituteMacroses(string expression, QuestionnaireDocument questionnaire);
    }
}