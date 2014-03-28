namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    public struct AddedRosterInstanceDto
    {
        public RosterInstanceIdentity Instance { get; private set; }
        public int? SortIndex { get; private set; }

        public AddedRosterInstanceDto(RosterInstanceIdentity instance, int? sortIndex)
            : this()
        {
            this.Instance = instance;
            this.SortIndex = sortIndex;
        }
    }
}