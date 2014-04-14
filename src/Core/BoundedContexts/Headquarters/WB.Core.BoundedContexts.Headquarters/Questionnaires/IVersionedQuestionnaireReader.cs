using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires
{
    public interface IVersionedQuestionnaireReader
    {
        QuestionnaireDocument Get(string id, long version);
    }
}