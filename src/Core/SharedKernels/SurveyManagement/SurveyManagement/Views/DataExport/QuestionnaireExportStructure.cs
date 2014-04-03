using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class QuestionnaireExportStructure : IVersionedView
    {
        public QuestionnaireExportStructure()
        {
            this.HeaderToLevelMap = new Dictionary<Guid, HeaderStructureForLevel>();
        }
        public Guid QuestionnaireId { get; set; }
        public Dictionary<Guid, HeaderStructureForLevel> HeaderToLevelMap { get; set; }
        public long Version { get; set; }
    }
}
