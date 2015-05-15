using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class RosterScopeTemplate
    {
        public RosterScopeTemplate(RosterScopeTemplateModel rosterScopeModel)
        {
            this.Model = rosterScopeModel;
        }

        public RosterScopeTemplateModel Model { get; private set; }
    }
}
