using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class HeaderStructureForLevel
    {
        public Guid LevelId { get; set; }
        public string LevelName { get; set; }
        public LabelItem[] LevelLabels { get; set; }
        public string LevelIdColumnName { get; set; }
        public IDictionary<Guid, ExportedHeaderItem> HeaderItems { get; set; }

        public HeaderStructureForLevel()
        {
            this.HeaderItems = new Dictionary<Guid, ExportedHeaderItem>();
        }
    }
}
