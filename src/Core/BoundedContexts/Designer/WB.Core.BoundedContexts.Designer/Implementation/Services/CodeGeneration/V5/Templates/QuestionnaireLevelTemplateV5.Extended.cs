using System.Collections.Generic;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates
{
    public partial class QuestionnaireLevelTemplateV5
    {
        public QuestionnaireLevelTemplateV5(
            QuestionnaireLevelTemplateModel model,
            List<LookupTableTemplateModel> lookupTables)
        {
            this.Model = model;
            this.LookupTables = lookupTables;
        }

        public QuestionnaireLevelTemplateModel Model { get; private set; }

        public List<LookupTableTemplateModel> LookupTables { get; private set; }
    }
}
