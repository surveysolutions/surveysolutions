using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class LevelTemplate

    {
        public LevelModel Model { get; }
        public CodeGenerationModel Processor { get; }

        public LevelTemplate(LevelModel model, CodeGenerationModel processor)
        {
            this.Model = model;
            this.Processor = processor;
        }
    }
}
