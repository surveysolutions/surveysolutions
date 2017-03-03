using System;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public class VariableModel
    {
        public Guid Id { set; get; }
        public string Variable { set; get; }
        public string ClassName { get; set; }
        public string ExpressionMethod { get; set; }
        public string TypeName { get; set; }
        public RosterScope RosterScope { get; set; }
    }
}