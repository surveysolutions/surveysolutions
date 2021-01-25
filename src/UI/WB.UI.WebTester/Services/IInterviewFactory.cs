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

        Task<CreationResult> CreateInterview(Guid designerToken, Guid originalInterviewId);

        Task<CreationResult> CreateInterview(Guid designerToken, int scenarioId);
    }
}
