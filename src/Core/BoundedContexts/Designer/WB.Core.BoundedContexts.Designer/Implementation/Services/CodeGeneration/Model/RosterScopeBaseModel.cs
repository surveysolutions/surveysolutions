using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public abstract class RosterScopeBaseModel
    {
        protected RosterScopeBaseModel(bool generateEmbeddedExpressionMethods, RosterScopeBaseModel parentScope, 
            string generatedRosterScopeName, string generatedTypeName, List<GroupTemplateModel> groups, 
            List<QuestionTemplateModel> questions, List<RosterTemplateModel> rosters, List<Guid> rosterScope)
        {
            GenerateEmbeddedExpressionMethods = generateEmbeddedExpressionMethods;
            ParentScope = parentScope;
            GeneratedRosterScopeName = generatedRosterScopeName;
            GeneratedTypeName = generatedTypeName;
            Groups = groups;
            Questions = questions;
            Rosters = rosters;
            RosterScope = rosterScope;
        }

        protected RosterScopeBaseModel(){}

        public bool GenerateEmbeddedExpressionMethods { get; set; }
        public RosterScopeBaseModel ParentScope { set; get; }
        public string GeneratedRosterScopeName { get; set; }
        public string GeneratedTypeName { set; get; }
        public List<QuestionTemplateModel> Questions { get; set; }
        public List<GroupTemplateModel> Groups { get; set; }
        public List<RosterTemplateModel> Rosters { get; set; }
        public int Version { get; set; }
        public List<Guid> RosterScope { set; get; }

        public IEnumerable<QuestionTemplateModel> GetAllQuestionsToTop()
        {
            return this.ParentScope != null ? this.Questions.Union(this.ParentScope.GetAllQuestionsToTop()) : this.Questions;
        }

        public IEnumerable<RosterTemplateModel> GetAllRostersToTop()
        {
            return this.ParentScope != null ? this.Rosters.Union(this.ParentScope.GetAllRostersToTop()) : this.Rosters;
        }
    }
}