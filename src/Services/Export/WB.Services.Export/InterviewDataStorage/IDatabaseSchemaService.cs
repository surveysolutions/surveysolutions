using System.Threading;
using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IDatabaseSchemaService
    {
        Task CreateQuestionnaireDbStructureAsync(QuestionnaireId questionnaireId, CancellationToken cancellationToken);
        void CreateQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument);
        bool TryDropQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument);
    }
}
