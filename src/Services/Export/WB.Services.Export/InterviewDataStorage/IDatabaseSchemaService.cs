using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IDatabaseSchemaService
    {
        void CreateOrRemoveSchema(QuestionnaireDocument questionnaire);

        bool TryDropQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument);
    }
}
