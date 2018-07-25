namespace WB.Core.SharedKernels.Enumerator.Views
{
    public enum TransferingStatus
    {
        WaitingDevice = 1,
        Failed,
        Transferring,
        CompletedWithErrors,
        Completed,
        Aborted
    }
}
