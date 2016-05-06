using System.Collections.Generic;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V9.Templates
{
    public partial class RosterScopeTemplateV9
    {
        public RosterScopeTemplateV9(
            RosterScopeTemplateModel rosterScopeModel,
            List<LookupTableTemplateModel> lookupTables)
        {
            this.Model = rosterScopeModel;
            this.LookupTables = lookupTables;
        }

        public RosterScopeTemplateModel Model { get; private set; }

        public List<LookupTableTemplateModel> LookupTables { get; private set; }
    }
}
