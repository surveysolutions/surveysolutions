using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class QuestionnaireExportStructure : IReadSideRepositoryEntity
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
