using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Versions;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class RosterScopeTemplate
    {
        public RosterScopeTemplate(KeyValuePair<string, List<RosterTemplateModel>> rosterScope, QuestionnaireExecutorTemplateModel executorModel, IVersionParameters versionParameters)
        {
            this.VersionParameters = versionParameters;
            this.Model = new RosterScopeTemplateModel(rosterScope, executorModel);
        }

        protected RosterScopeTemplateModel Model { get; private set; }
        protected IVersionParameters VersionParameters { get; private set; }
    }
}
