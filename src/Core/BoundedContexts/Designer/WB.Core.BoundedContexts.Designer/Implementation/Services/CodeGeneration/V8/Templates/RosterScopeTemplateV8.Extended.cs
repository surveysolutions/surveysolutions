using System.Collections.Generic;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V8.Templates
{
    public partial class RosterScopeTemplateV8
    {
        public RosterScopeTemplateV8(
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
