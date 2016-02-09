using System.Collections.Generic;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V6.Templates
{
    public partial class LookupTablesTemplateV6
    {
        public LookupTablesTemplateV6(List<LookupTableTemplateModel> lookupTables)
        {
            this.LookupTables = lookupTables;
        }

        public List<LookupTableTemplateModel> LookupTables { get; private set; }
    }
}
