namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    public struct AddedRosterRowDto
    {
        public RosterRowIdentity Row { get; private set; }
        public int? SortIndex { get; private set; }

        public AddedRosterRowDto(RosterRowIdentity row, int? sortIndex)
            : this()
        {
            this.Row = row;
            this.SortIndex = sortIndex;
        }
    }
}