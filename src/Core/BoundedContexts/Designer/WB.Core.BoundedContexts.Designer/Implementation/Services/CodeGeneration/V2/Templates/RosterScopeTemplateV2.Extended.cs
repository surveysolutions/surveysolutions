using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V2.Templates
{
    public partial class RosterScopeTemplateV2
    {
        public RosterScopeTemplateV2(RosterScopeTemplateModel rosterScopeModel)
        {
            this.Model = rosterScopeModel;
        }

        public RosterScopeTemplateModel Model { get; private set; }
    }
}
