using System;
using System.Collections.Generic;

namespace WB.Services.Export.Interview
{
    public class HeaderStructureForLevel
    {
        public ValueVector<Guid> LevelScopeVector { get; set; }
        public string LevelName { get; set; }
        public LabelItem[]? LevelLabels { get; set; }
        public string LevelIdColumnName { get; set; }

        public string[] ReferencedNames { get; set; } = new string[0];
        public bool IsTextListScope { get; set; }

        public IDictionary<Guid, IExportedHeaderItem> HeaderItems { get; set; }
        public IDictionary<Guid, ReusableLabels> ReusableLabels { get; set; }

        public HeaderStructureForLevel(ValueVector<Guid> levelScopeVector, string levelIdColumnName, string levelName)
        {
            LevelScopeVector = levelScopeVector;
            LevelName = levelName;
            //LevelLabels = levelLabels;
            LevelIdColumnName = levelIdColumnName;
            //ReferencedNames = referencedNames;
            this.HeaderItems = new Dictionary<Guid, IExportedHeaderItem>();
            this.ReusableLabels = new Dictionary<Guid, ReusableLabels>();
        }
    }

    public class ReusableLabels
    {
        public ReusableLabels(string name, LabelItem[] labels)
        {
            Name = name;
            Labels = labels;
        }

        public string Name { get; set; }
        public LabelItem[] Labels { get; set; }
    }
}
