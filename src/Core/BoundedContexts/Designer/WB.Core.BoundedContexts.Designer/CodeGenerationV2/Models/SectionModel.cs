using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
{
    public class SectionModel
    {
        public SectionModel(string? variable, RosterScope rosterScope)
        {
            Variable = variable;
            RosterScope = rosterScope;
        }

        public string? Variable { set; get; }
        public RosterScope RosterScope { get; set; }
    }
}
