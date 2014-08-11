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
                Rosters = rosterScope.Value,

                ParentTypeName = rosterScope.Value[0].GetParentScope().GetTypeName()
            };
        }
    }

    public class RosterScopeTemplateModel
    {
        public string GeneratedTypeName { set; get; }
        public List<RosterTemplateModel> Rosters { set; get; }

        public string ParentTypeName { set; get; }

        public IRosterScope GetParentScope()
        {
            return Rosters[0].GetParentScope();
        }
    }
}
