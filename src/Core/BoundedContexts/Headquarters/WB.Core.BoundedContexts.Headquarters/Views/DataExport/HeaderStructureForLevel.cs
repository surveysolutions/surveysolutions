using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
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
        
        public HeaderStructureForLevel()
        {
            this.HeaderItems = new Dictionary<Guid, IExportedHeaderItem>();
        }

        public bool ContainsQuestion(Guid questionId)
        {
           return this.HeaderItems.ContainsKey(questionId);
        }
    }
}
