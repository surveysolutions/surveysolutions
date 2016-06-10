using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Factories
{
    public interface IQuestionnaireRosterStructureFactory
    {
        QuestionnaireRosterStructure CreateQuestionnaireRosterStructure(QuestionnaireDocument questionnaire, long version);
    }
}
