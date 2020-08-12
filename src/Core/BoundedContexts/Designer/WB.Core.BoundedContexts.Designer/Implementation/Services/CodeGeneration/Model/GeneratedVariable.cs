namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class GeneratedVariable
    {

        public GeneratedVariable()
        {
            Name = string.Empty;
            Type = string.Empty;
            Visibility = "private";
        }

        public GeneratedVariable(string name, string type, string? initialization = null, string visibility = "private", bool isStatic = true, bool isReadonly = true)
        {
            this.Type = type;
            this.Name = name;
            this.Initialization = initialization;
            this.Visibility = visibility;
            this.IsStatic = isStatic;
            this.IsReadonly = isReadonly;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public string Visibility { get; set; }
        public bool IsStatic { get; set; }
        public bool IsReadonly { get; set; }
        public string? Initialization { get; set; }
    }
}
