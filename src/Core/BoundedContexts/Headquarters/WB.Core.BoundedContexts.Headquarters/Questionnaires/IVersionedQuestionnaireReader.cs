using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires
{
    public interface IVersionedQuestionnaireReader
    {
        QuestionnaireDocument Get(string id, long version);
    }
}