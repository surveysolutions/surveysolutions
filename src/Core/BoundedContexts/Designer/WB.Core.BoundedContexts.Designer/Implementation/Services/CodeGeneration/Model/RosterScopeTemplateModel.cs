using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class RosterScopeTemplateModel : RosterScopeBaseModel
    {
        public RosterScopeTemplateModel(KeyValuePair<string, List<RosterTemplateModel>> rosterScope,
            QuestionnaireExecutorTemplateModel executorModel)
        {
            this.GeneratedTypeName = rosterScope.Key;
            this.RostersInScope = rosterScope.Value;

            this.ParentTypeName = rosterScope.Value[0].ParentScope.GeneratedTypeName;
            this.ParentScope = this.RostersInScope.First().ParentScope;

            this.Questions = rosterScope.Value.SelectMany(r => r.Questions).ToList();
            this.Groups = rosterScope.Value.SelectMany(r => r.Groups).ToList();
            this.Rosters = rosterScope.Value.SelectMany(r => r.Rosters).ToList();

            this.ExecutorModel = executorModel;
        }

        public QuestionnaireExecutorTemplateModel ExecutorModel { private set; get; }

        public string ParentTypeName { set; get; }

        public List<RosterTemplateModel> RostersInScope { set; get; }
    }
}