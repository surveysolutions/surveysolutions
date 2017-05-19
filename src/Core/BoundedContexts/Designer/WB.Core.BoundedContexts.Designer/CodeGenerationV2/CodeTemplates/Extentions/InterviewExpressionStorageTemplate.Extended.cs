using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class InterviewExpressionStorageTemplate
    {
        public InterviewExpressionStorageTemplate(CodeGenerationModel model)
        {
            this.Model = model;
        }

        public CodeGenerationModel Model { get; private set; }

        protected LevelTemplate CreateLevelTemplate(LevelModel level, CodeGenerationModel model)
        {
            return new LevelTemplate(level, model);
        }
    }
}