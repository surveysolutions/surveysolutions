using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public abstract class RosterScopeBaseModel
    {
        protected RosterScopeBaseModel(
            string rosterScopeName, 
            string typeName, 
            List<GroupTemplateModel> groups, 
            List<QuestionTemplateModel> questions, 
            List<RosterTemplateModel> rosters, 
            List<Guid> rosterScope,
            string parentTypeName)
        {
            this.RosterScopeName = rosterScopeName;
            this.TypeName = typeName;
            Groups = groups;
            Questions = questions;
            Rosters = rosters;
            RosterScope = rosterScope;
            this.ParentTypeName = parentTypeName;
        }

        protected RosterScopeBaseModel()
        {
        }

        public string RosterScopeName { get; set; }

        public string TypeName { set; get; }

        public string ParentTypeName { set; get; }

        public List<QuestionTemplateModel> Questions { get; set; }

        public List<GroupTemplateModel> Groups { get; set; }

        public List<RosterTemplateModel> Rosters { get; set; }

        public List<Guid> RosterScope { set; get; }
    }
}