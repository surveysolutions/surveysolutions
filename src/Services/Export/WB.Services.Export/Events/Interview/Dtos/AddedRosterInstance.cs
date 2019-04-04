namespace WB.Services.Export.Events.Interview.Dtos
{
    /// <remarks>Make sure not to reuse this class on read or write side. Use your own copies.</remarks>
    public class AddedRosterInstance : RosterInstance
    {
        public int? SortIndex { get; set; }
    }
}
