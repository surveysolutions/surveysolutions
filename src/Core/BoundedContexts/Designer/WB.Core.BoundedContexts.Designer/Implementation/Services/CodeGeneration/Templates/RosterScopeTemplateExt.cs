using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class RosterScopeTemplate
    {
        protected RosterScopeTemplateModel RosterScopeModel { private set; get; }

        public RosterScopeTemplate(KeyValuePair<string, List<RosterTemplateModel>> rosterScope)
        {
            //todo: move logic from template to model and merge rosters

            this.RosterScopeModel = new RosterScopeTemplateModel()
            {
                GeneratedTypeName = rosterScope.Key,
                RostersInScope = rosterScope.Value,

                ParentTypeName = rosterScope.Value[0].GetParentScope().GetTypeName()
            };
        }
    }
}
