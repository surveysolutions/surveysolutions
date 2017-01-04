using System.Linq;
using System.Text;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class LazyHashSetGeneratedVariable : GeneratedVariable
    {
        public LazyHashSetGeneratedVariable(string name, string type, string[] values, bool isStatic = true, bool isReadonly = true)
        {
            this.IsReadonly = isReadonly;
            this.Name = $"{CodeGenerator.PrivateFieldsPrefix}{name}Options";
            this.Type = $"Lazy<HashSet<{type}>>";
            this.IsStatic = isStatic;
            this.Visibility = "private";
            this.Initialization = this.BuildInitForHashSet(type, values);
        }

        private string BuildInitForHashSet(string type, string[] parentOptions)
        {
            var sb = new StringBuilder();

            bool isFirstLine = true;

            foreach (var parentOptionSubList in parentOptions.Batch(500))
            {
                var options = string.Join(",", parentOptionSubList.Select(x => $"{x}"));

                if (!isFirstLine)
                {
                    sb.Append($"\r\n.Union(new {type}[] {{ {options} }})");
                }
                else
                {
                    sb.AppendLine($"new {type}[] {{ {options} }}");
                    isFirstLine = false;
                }
            }

            return $" = new Lazy<HashSet<{type}>>(() => new HashSet<{type}>({sb}))";
        }
    }
}