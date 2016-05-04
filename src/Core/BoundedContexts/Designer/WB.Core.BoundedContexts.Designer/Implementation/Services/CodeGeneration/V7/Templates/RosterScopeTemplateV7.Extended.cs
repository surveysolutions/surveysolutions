using System.Collections.Generic;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V6.Templates;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V7.Templates
{
    public partial class RosterScopeTemplateV7
    {
        public RosterScopeTemplateV7(
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
