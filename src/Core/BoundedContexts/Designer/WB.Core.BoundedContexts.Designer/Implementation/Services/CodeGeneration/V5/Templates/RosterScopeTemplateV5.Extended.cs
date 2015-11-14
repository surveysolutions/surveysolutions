using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates
{
    public partial class RosterScopeTemplateV5
    {
        public RosterScopeTemplateV5(RosterScopeTemplateModel rosterScopeModel)
        {
            this.Model = rosterScopeModel;
        }

        public RosterScopeTemplateModel Model { get; private set; }
    }
}
