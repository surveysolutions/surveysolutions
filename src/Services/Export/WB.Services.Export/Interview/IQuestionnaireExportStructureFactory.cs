using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Interview
{
    internal interface IQuestionnaireExportStructureFactory
    {
        Task<QuestionnaireExportStructure> GetQuestionnaireExportStructure(string questionnaireId, string tenantId);
    }
}
