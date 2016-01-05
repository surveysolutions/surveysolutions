using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public abstract class RosterScopeBaseModel
    {
        protected RosterScopeBaseModel(
            string generatedRosterScopeName, 
            string generatedTypeName, 
            List<GroupTemplateModel> groups, 
            List<QuestionTemplateModel> questions, 
            List<RosterTemplateModel> rosters, 
            List<Guid> rosterScope,
            string parentGeneratedTypeName)
        {
            GeneratedRosterScopeName = generatedRosterScopeName;
            GeneratedTypeName = generatedTypeName;
            Groups = groups;
            Questions = questions;
            Rosters = rosters;
            RosterScope = rosterScope;
            ParentGeneratedTypeName = parentGeneratedTypeName;
        }

        protected RosterScopeBaseModel()
        {
        }

        public string GeneratedRosterScopeName { get; set; }

        public string GeneratedTypeName { set; get; }

        public string ParentGeneratedTypeName { set; get; }

        public List<QuestionTemplateModel> Questions { get; set; }

        public List<GroupTemplateModel> Groups { get; set; }

        public List<RosterTemplateModel> Rosters { get; set; }

        public List<Guid> RosterScope { set; get; }
    }
}