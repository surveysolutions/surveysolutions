using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class RosterScopeTemplateModel : IRosterScope
    {
        public RosterScopeTemplateModel(KeyValuePair<string, List<RosterTemplateModel>> rosterScope,
            QuestionnaireExecutorTemplateModel executorModel)
        {
            this.GeneratedTypeName = rosterScope.Key;
            this.RostersInScope = rosterScope.Value;

            this.ParentTypeName = rosterScope.Value[0].GetParentScope().GetTypeName();

            this.Questions = rosterScope.Value.SelectMany(r => r.Questions).ToList();
            this.Groups = rosterScope.Value.SelectMany(r => r.Groups).ToList();
            this.Rosters = rosterScope.Value.SelectMany(r => r.Rosters).ToList();

            this.ExecutorModel = executorModel;
        }

        public string GeneratedRosterScopeName { get; set; }
        public string GeneratedTypeName { set; get; }

        public List<QuestionTemplateModel> Questions { get; set; }
        public List<GroupTemplateModel> Groups { get; set; }
        public List<RosterTemplateModel> Rosters { set; get; }

        public QuestionnaireExecutorTemplateModel ExecutorModel { private set; get; }

        public string ParentTypeName { set; get; }


        public List<RosterTemplateModel> RostersInScope { set; get; }

        public IRosterScope GetParentScope()
        {
            return this.RostersInScope.First().GetParentScope();
        }

        public string GetTypeName()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<QuestionTemplateModel> GetAllQuestionsToTop()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RosterTemplateModel> GetAllRostersToTop()
        {
            throw new NotImplementedException();
        }

        public List<Guid> GetRosterScope()
        {
            throw new NotImplementedException();
        }
    }
}