using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class RosterTemplateModel : RosterScopeBaseModel
    {
        public RosterTemplateModel()
        {
            this.Questions = new List<QuestionTemplateModel>();
            this.Groups = new List<GroupTemplateModel>();
            this.Rosters = new List<RosterTemplateModel>();
        }

        public string Conditions { set; get; }

        public string GeneratedConditionsMethodName { set; get; }
        public string GeneratedIdName { set; get; }
        public string GeneratedStateName { set; get; }

        public Guid Id { set; get; }

        public string VariableName { set; get; }
    }
}