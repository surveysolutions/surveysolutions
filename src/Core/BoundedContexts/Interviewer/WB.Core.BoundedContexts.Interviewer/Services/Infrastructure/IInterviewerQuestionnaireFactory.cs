using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Interviewer.Services.Infrastructure
{
    public interface IInterviewerQuestionnaireFactory
    {
        Task StoreQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, string questionnaireDocument,
            bool census);

        Task RemoveQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity);

        Task StoreQuestionnaireAssemblyAsync(QuestionnaireIdentity questionnaireIdentity, byte[] assembly);

        List<QuestionnaireIdentity> GetCensusQuestionnaireIdentities();

        bool IsQuestionnaireExists(QuestionnaireIdentity questionnaireIdentity);

        bool IsQuestionnaireAssemblyExists(QuestionnaireIdentity questionnaireIdentity);
    }
}