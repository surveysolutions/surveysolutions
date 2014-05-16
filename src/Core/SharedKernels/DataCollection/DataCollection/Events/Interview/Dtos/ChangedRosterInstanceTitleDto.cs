namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    public class ChangedRosterInstanceTitleDto
    {
        public RosterInstance RosterInstance { get; private set; }
        public string Title { private set; get; }

        public ChangedRosterInstanceTitleDto(RosterInstance rosterInstance, string title)
        {
            this.RosterInstance = rosterInstance;
            this.Title = title;
        }
    }
}