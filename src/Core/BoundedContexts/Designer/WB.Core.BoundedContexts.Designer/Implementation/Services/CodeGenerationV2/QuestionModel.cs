using System;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public class QuestionModel
    {
        public Guid Id { set; get; }
        public string Variable { set; get; }
        public string ClassName { get; set; }

        public string TypeName { get; set; }
        public RosterScope RosterScope { get; set; }
    }

    public class StaticTextModel
    {
        public Guid Id { set; get; }
        public string Variable { set; get; }
        public string ClassName { get; set; }
        public RosterScope RosterScope { get; set; }
    }
}