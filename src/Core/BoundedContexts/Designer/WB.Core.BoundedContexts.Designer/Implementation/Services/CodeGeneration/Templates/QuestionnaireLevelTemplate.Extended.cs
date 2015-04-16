using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class QuestionnaireLevelTemplate
    {
        public QuestionnaireLevelTemplate(
            QuestionnaireLevelTemplateModel model)
        {
            Model = model;
        }

        public QuestionnaireLevelTemplateModel Model { get; private set; }
    }
}
