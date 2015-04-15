using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Versions;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class QuestionnaireLevelTemplate
    {
        public QuestionnaireLevelTemplate(QuestionnaireLevelTemplateModel model, IVersionParameters versionParameters)
        {
            this.Model = model;
            this.VersionParameters = versionParameters;
        }

        protected QuestionnaireLevelTemplateModel Model { get; private set; }
        protected IVersionParameters VersionParameters { get; private set; }
    }
}
