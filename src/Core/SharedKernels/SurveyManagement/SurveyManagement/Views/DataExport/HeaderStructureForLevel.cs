using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class HeaderStructureForLevel
    {
        public Guid LevelId { get; set; }
        public string LevelName { get; set; }
        public LabelItem[] LevelLabels { get; set; }
        public string LevelIdColumnName { get; set; }

        public string[] ReferencedNames { get; set; }
        public bool IsTextListScope { get; set; }

        public IDictionary<Guid, ExportedHeaderItem> HeaderItems { get; set; }
        
        public HeaderStructureForLevel()
        {
            this.HeaderItems = new Dictionary<Guid, ExportedHeaderItem>();
        }
    }
}
