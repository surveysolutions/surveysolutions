using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
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
