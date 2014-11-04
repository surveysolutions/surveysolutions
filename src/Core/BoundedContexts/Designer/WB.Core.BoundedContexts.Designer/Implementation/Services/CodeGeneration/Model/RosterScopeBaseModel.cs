using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public abstract class RosterScopeBaseModel
    {
        public RosterScopeBaseModel ParentScope { set; get; }

        public string GeneratedRosterScopeName { get; set; }
        public string GeneratedTypeName { set; get; }
        public List<QuestionTemplateModel> Questions { get; set; }
        public List<GroupTemplateModel> Groups { get; set; }
        public List<RosterTemplateModel> Rosters { get; set; }

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