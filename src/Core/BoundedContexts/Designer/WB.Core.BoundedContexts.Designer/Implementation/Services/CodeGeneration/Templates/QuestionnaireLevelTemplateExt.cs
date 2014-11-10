using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class QuestionnaireLevelTemplate
    {
        protected QuestionnaireLevelTemplateModel Model { private set; get; }

        public QuestionnaireLevelTemplate(QuestionnaireLevelTemplateModel model)
        {
            this.Model = model;
        }
    }
}
