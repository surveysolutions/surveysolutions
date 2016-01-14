using System.Collections.Generic;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates
{
    public partial class LookupTablesTemplateV5
    {
        public LookupTablesTemplateV5(List<LookupTableTemplateModel> lookupTables)
        {
            this.LookupTables = lookupTables;
        }

        public List<LookupTableTemplateModel> LookupTables { get; private set; }
    }
}
