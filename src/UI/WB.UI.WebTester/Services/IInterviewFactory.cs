using System;
using System.Threading.Tasks;

namespace WB.UI.WebTester.Services
{
    public enum CreationResult
    {
        DataRestored,
        EmptyCreated
    }

    public interface IInterviewFactory
    {
        Task CreateInterview(Guid designerToken);

        Task<CreationResult> CreateInterview(Guid designerToken, Guid originalInterviewId);

        Task<CreationResult> CreateInterview(Guid designerToken, int scenarioId);
    }
}
