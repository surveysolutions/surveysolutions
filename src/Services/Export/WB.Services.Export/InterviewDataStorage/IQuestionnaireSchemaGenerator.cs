using System.Threading.Tasks;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.InterviewDataStorage
{
    public interface IQuestionnaireSchemaGenerator
    {
        void CreateQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument);
        void DropQuestionnaireDbStructure(QuestionnaireDocument questionnaireDocument);
        Task DropTenantSchemaAsync(string tenant);
    }
}
