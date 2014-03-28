namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    public struct ChangedRosterInstanceTitleDto
    {
        public RosterInstanceIdentity Instance { get; private set; }
        public string Title { private set; get; }

        public ChangedRosterInstanceTitleDto(RosterInstanceIdentity instance, string title)
            : this()
        {
            this.Instance = instance;
            this.Title = title;
        }
    }
}