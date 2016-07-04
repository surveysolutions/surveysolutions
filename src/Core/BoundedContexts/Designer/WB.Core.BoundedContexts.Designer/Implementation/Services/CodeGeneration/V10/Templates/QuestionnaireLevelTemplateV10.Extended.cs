using System.Collections.Generic;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V10.Templates
{
    public partial class QuestionnaireLevelTemplateV10
    {
        public QuestionnaireLevelTemplateV10(
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
