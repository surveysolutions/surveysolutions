namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    public struct ChangedRosterRowTitleDto
    {
        public RosterRowIdentity Row { get; private set; }
        public string Title { private set; get; }

        public ChangedRosterRowTitleDto(RosterRowIdentity row, string title)
            : this()
        {
            this.Row = row;
            this.Title = title;
        }
    }
}