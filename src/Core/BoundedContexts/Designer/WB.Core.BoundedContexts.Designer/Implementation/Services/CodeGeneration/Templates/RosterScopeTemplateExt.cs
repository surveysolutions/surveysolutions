﻿using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class RosterScopeTemplate
    {
        protected RosterScopeTemplateModel Model { private set; get; }

        public RosterScopeTemplate(KeyValuePair<string, List<RosterTemplateModel>> rosterScope, QuestionnaireExecutorTemplateModel executorModel)
        {
            this.Model = new RosterScopeTemplateModel(rosterScope, executorModel);
        }
    }
}
