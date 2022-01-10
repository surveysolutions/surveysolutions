using System.Diagnostics;

namespace WB.Core.BoundedContexts.Designer.ImportExport.Models
{
    [DebuggerDisplay("Roster {Id}")]
    public class Roster : Group
    {
        public RosterDisplayMode DisplayMode { get; set; }

        public string? RosterSizeQuestion { get; set; }

        public string? RosterTitleQuestion { get; set; }

        public RosterSizeSourceType RosterSizeSource { get; set; }

        public FixedRosterTitle[]? FixedRosterTitles { get; set; }
    }
}
