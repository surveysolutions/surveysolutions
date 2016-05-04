using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V2.Templates
{
    public partial class QuestionnaireLevelTemplateV2
    {
        public QuestionnaireLevelTemplateV2(
            QuestionnaireLevelTemplateModel model)
        {
            this.Model = model;
        }

        public QuestionnaireLevelTemplateModel Model { get; private set; }
    }
}
