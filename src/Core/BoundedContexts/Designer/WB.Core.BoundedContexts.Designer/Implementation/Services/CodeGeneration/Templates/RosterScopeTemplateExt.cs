using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class RosterScopeTemplate
    {
        protected RosterScopeTemplateModel Model { private set; get; }

        public RosterScopeTemplate(KeyValuePair<string, List<RosterTemplateModel>> rosterScope)
        {
            //todo: move logic from template to model and merge rosters

            this.Model = new RosterScopeTemplateModel(rosterScope);
        }
    }
}
