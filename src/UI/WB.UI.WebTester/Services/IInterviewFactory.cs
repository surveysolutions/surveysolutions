using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.UI.WebTester.Services
{
    public enum CreationResult
    {
        DataRestored,
        EmptyCreated
    }

    public interface IInterviewFactory
    {
        Task<QuestionnaireIdentity> ImportQuestionnaireAndCreateInterview(Guid designerToken);

        Task<CreationResult> ImportQuestionnaireAndCreateInterview(Guid designerToken, Guid originalInterviewId);

        Task<CreationResult> ImportQuestionnaireAndCreateInterview(Guid designerToken, int scenarioId);
    }
}
