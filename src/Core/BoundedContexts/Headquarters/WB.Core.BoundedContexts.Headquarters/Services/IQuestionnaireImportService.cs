using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IQuestionnaireImportService
    {
        Task<QuestionnaireImportResult> Import(Guid questionnaireId, string name, bool isCensusMode);
    }
}