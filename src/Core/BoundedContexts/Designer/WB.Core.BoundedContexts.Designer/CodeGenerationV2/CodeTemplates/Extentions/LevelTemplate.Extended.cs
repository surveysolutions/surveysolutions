using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class LevelTemplate

    {
        public LevelModel Model { get; }
        public ExpressionStorageModel Storage { get; }

        public LevelTemplate(LevelModel model, ExpressionStorageModel storage)
        {
            this.Model = model;
            this.Storage = storage;
        }
    }
}
