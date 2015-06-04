using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public abstract class RosterScopeBaseModel
    {
        private IEnumerable<RosterTemplateModel> allRostersToTop;

        protected RosterScopeBaseModel(
            RosterScopeBaseModel parentScope, 
            string generatedRosterScopeName, 
            string generatedTypeName, 
            List<GroupTemplateModel> groups, 
            List<QuestionTemplateModel> questions, 
            List<RosterTemplateModel> rosters, 
            List<Guid> rosterScope, 
            bool areRowSpecificVariablesPresent, 
            bool isIRosterLevelInherited, 
            string rosterType)
        {
            ParentScope = parentScope;
            GeneratedRosterScopeName = generatedRosterScopeName;
            GeneratedTypeName = generatedTypeName;
            Groups = groups;
            Questions = questions;
            Rosters = rosters;
            RosterScope = rosterScope;
            AreRowSpecificVariablesPresent = areRowSpecificVariablesPresent;
            IsIRosterLevelInherited = isIRosterLevelInherited;
            RosterType = rosterType;
        }

        protected RosterScopeBaseModel()
        {
        }

        public RosterScopeBaseModel ParentScope { set; get; }

        public string GeneratedRosterScopeName { get; set; }

        public string GeneratedTypeName { set; get; }

        public List<QuestionTemplateModel> Questions { get; set; }

        public List<GroupTemplateModel> Groups { get; set; }

        public List<RosterTemplateModel> Rosters { get; set; }

        public List<Guid> RosterScope { set; get; }

        public bool AreRowSpecificVariablesPresent { get;private set; }

        public bool IsIRosterLevelInherited { get; private set; }

        public string RosterType { get; private set; }

        public IEnumerable<QuestionTemplateModel> GetAllQuestionsToTop()
        {
            return this.ParentScope != null ? this.Questions.Union(this.ParentScope.GetAllQuestionsToTop()) : this.Questions;
        }

        public IEnumerable<RosterTemplateModel> GetAllRostersToTop()
        {
            if (allRostersToTop == null)
            {
                allRostersToTop = this.ParentScope != null
                    ? this.Rosters.Union(this.ParentScope.GetAllRostersToTop())
                    : this.Rosters;
            }
            return allRostersToTop;
        }
    }
}