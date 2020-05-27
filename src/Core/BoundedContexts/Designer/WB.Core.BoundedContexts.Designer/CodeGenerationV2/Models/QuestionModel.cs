using System;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
{
    public class QuestionModel
    {
        public QuestionModel(Guid id, string variable, string typeName, RosterScope rosterScope)
        {
            Id = id;
            Variable = variable;
            TypeName = typeName;
            RosterScope = rosterScope;
        }

        public Guid Id { set; get; }
        public string Variable { set; get; }

        public string TypeName { get; set; }
        public RosterScope RosterScope { get; set; }
    }
}
