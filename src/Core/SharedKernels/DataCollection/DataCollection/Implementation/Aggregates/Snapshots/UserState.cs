namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots
{
    public class UserState
    {
        public bool IsUserLockedBySupervisor { get; set; }
        public bool IsUserLockedByHQ { get; set; }
    }
}