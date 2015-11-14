using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V5.Templates
{
    public partial class QuestionnaireLevelTemplateV5
    {
        public QuestionnaireLevelTemplateV5(
            QuestionnaireLevelTemplateModel model)
        {
            this.Model = model;
        }

        public QuestionnaireLevelTemplateModel Model { get; private set; }
    }
}
