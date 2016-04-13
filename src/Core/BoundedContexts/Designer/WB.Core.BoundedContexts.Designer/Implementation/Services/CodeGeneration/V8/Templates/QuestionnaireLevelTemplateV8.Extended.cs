using System.Collections.Generic;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V8.Templates
{
    public partial class QuestionnaireLevelTemplateV8
    {
        public QuestionnaireLevelTemplateV8(
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
