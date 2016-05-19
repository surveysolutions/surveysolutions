﻿using System.Collections.Generic;

using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V10.Templates
{
    public partial class RosterScopeTemplateV10
    {
        public RosterScopeTemplateV10(
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
