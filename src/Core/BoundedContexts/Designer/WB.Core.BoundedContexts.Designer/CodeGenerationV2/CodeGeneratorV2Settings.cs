namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    public class CodeGeneratorV2Settings
    {
        public CodeGeneratorV2Settings(int targetVersion)
        {
            this.TargetVersion = targetVersion;
        }

        public int TargetVersion { get; set; }
    }
}