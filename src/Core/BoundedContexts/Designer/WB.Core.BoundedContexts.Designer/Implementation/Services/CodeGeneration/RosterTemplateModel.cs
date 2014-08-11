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

        public List<QuestionTemplateModel> Questions {  set; get; }
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

        public IEnumerable<QuestionTemplateModel> GetQuestions()
        {
            return ParentScope != null ? this.Questions.Union(ParentScope.GetQuestions()) : this.Questions ;
        }

        public IEnumerable<RosterTemplateModel> GetRosters()
        {
            return ParentScope != null ? this.Rosters.Union(ParentScope.GetRosters()) : this.Rosters;
        }
    }
}