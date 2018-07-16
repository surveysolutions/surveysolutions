using System.Threading.Tasks;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class OkResponse : ICommunicationMessage
    {
        public static Task<OkResponse> Task { get; } = System.Threading.Tasks.Task.FromResult(new OkResponse());
    }

    public class FailedResponse : ICommunicationMessage { }
}
