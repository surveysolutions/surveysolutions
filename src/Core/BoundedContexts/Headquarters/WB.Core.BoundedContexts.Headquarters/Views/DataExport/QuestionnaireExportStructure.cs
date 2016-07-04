using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.Views.DataExport
{
    public class QuestionnaireExportStructure
    {
        public QuestionnaireExportStructure()
        {
            this.HeaderToLevelMap = new Dictionary<ValueVector<Guid>, HeaderStructureForLevel>();
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<ValueVector<Guid>, HeaderStructureForLevel> HeaderToLevelMap { get; set; }
        public long Version { get; set; }
    }
}
