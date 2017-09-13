using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
{
    public class GroupedModel<T>
    {
        public List<T> Models { get; set; }
        public string ClassName { get; set; }
    }
}