using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class RosterTemplateModel : IRosterScope
    {
        public RosterTemplateModel()
        {
            this.Questions = new List<QuestionTemplateModel>();
            this.Groups = new List<GroupTemplateModel>();
            this.Rosters = new List<RosterTemplateModel>();
        }

        public Guid Id { set; get; }
        public string VariableName { set; get; }
        public string Conditions { set; get; }

        public string GeneratedTypeName { set; get; }
        public string GeneratedStateName { set; get; }
        public string GeneratedIdName { set; get; }
        public string GeneratedConditionsMethodName { set; get; }
        public string GeneratedRosterScopeName { set; get; }

        public List<QuestionTemplateModel> Questions { set; get; }
        public List<GroupTemplateModel> Groups { set; get; }
        public List<RosterTemplateModel> Rosters { set; get; }

        public List<Guid> RosterScope { set; get; }

        public List<Guid> GetRosterScope()
        {
            return this.RosterScope;
        }

        public IRosterScope ParentScope { set; get; }

        public IRosterScope GetParentScope()
        {
            return this.ParentScope;
        }

        public string GetTypeName()
        {
            return this.GeneratedTypeName;
        }

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