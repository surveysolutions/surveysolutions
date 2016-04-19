using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Entities;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface IQuestionnaireImportService
    {
        Task ImportQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument, string supportingAssembly);
    }
}