using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class InterviewExpressionStorageTemplate
    {
        public InterviewExpressionStorageTemplate(ExpressionStorageModel model)
        {
            this.Model = model;
        }

        public ExpressionStorageModel Model { get; private set; }

        protected LevelTemplate CreateLevelTemplate(LevelModel level, ExpressionStorageModel model)
        {
            return new LevelTemplate(level, model);
        }
    }
}