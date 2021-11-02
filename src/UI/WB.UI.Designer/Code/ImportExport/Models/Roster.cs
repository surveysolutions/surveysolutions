using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using WB.UI.Designer.Code.ImportExport.Models.Question;

namespace WB.UI.Designer.Code.ImportExport.Models
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
