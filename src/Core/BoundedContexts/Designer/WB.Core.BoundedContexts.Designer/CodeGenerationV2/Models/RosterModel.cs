using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
{
    public class RosterModel
    {
        public RosterModel(string variable, string className, RosterScope rosterScope)
        {
            Variable = variable;
            ClassName = className;
            RosterScope = rosterScope;
        }

        public string Variable { set; get; }
        public string ClassName { get; set; }
        public RosterScope RosterScope { get; set; }
    }
}
