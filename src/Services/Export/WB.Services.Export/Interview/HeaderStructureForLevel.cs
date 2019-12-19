using System;
using System.Collections.Generic;

namespace WB.Services.Export.Interview
{
    public class HeaderStructureForLevel
    {
        public ValueVector<Guid> LevelScopeVector { get; set; }
        public string LevelName { get; set; }
        public LabelItem[] LevelLabels { get; set; }
        public string LevelIdColumnName { get; set; }

        public string[] ReferencedNames { get; set; }
        public bool IsTextListScope { get; set; }

        public IDictionary<Guid, IExportedHeaderItem> HeaderItems { get; set; }
        public IDictionary<Guid, ReusableLabels> ReusableLabels { get; set; }

        public HeaderStructureForLevel()
        {
            this.HeaderItems = new Dictionary<Guid, IExportedHeaderItem>();
            this.ReusableLabels = new Dictionary<Guid, ReusableLabels>();
        }
    }

    public class ReusableLabels
    {
        public string Name { get; set; }
        public LabelItem[] Labels { get; set; }
    }
}
