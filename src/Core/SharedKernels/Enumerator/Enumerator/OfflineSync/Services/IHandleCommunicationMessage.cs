namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services
{
    public interface IHandleCommunicationMessage
    {
        void Register(IRequestHandler requestHandler);
    }
}