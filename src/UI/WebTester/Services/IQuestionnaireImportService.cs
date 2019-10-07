using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.WebTester.Services
{
    public interface IQuestionnaireImportService
    {
        Task<QuestionnaireIdentity> ImportQuestionnaire(Guid designerToken);
        void RemoveQuestionnaire(Guid designerToken);
    }
}