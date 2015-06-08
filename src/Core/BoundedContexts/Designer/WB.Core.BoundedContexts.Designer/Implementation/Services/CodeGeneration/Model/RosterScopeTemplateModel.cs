using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class RosterScopeTemplateModel
    {
        public RosterScopeTemplateModel(string rosterScopeType, List<RosterTemplateModel> rostersInScope, QuestionnaireExecutorTemplateModel executorModel)
        {
            GeneratedRosterScopeName =  String.Empty;
            GeneratedTypeName = rosterScopeType;
            Groups = rostersInScope.SelectMany(r => r.Groups).ToList();
            Questions =  rostersInScope.SelectMany(r => r.Questions).ToList();
            Rosters = rostersInScope.SelectMany(r => r.Rosters).ToList();
            RosterScope = new List<Guid>();
            AreRowSpecificVariablesPresent = executorModel.QuestionnaireLevelModel.AreRowSpecificVariablesPresent;
            IsIRosterLevelInherited = executorModel.QuestionnaireLevelModel.IsIRosterLevelInherited;
            RosterType = executorModel.QuestionnaireLevelModel.RosterType;
            this.RostersInScope = rostersInScope;
            this.ParentTypeName = rostersInScope[0].ParentScope.GeneratedTypeName;
            this.ExecutorModel = executorModel;
            this.AbstractConditionalLevelClassName =
                executorModel.QuestionnaireLevelModel.AbstractConditionalLevelClassName;
        }

        public RosterScopeTemplateModel ParentScope { set; get; }

        public string GeneratedRosterScopeName { get; set; }

        public string GeneratedTypeName { set; get; }

        public List<QuestionTemplateModel> Questions { get; set; }

        public List<GroupTemplateModel> Groups { get; set; }

        public List<RosterTemplateModel> Rosters { get; set; }

        public List<Guid> RosterScope { set; get; }

        public bool AreRowSpecificVariablesPresent { get; private set; }

        public bool IsIRosterLevelInherited { get; private set; }

        public string RosterType { get; private set; }

        public IEnumerable<QuestionTemplateModel> AllParentsQuestionsToTop  { set; get; }

        public IEnumerable<RosterTemplateModel> AllParentsRostersToTop { set; get; }

        public QuestionnaireExecutorTemplateModel ExecutorModel { private set; get; }

        public string ParentTypeName { set; get; }

        public List<RosterTemplateModel> RostersInScope { set; get; }

        public string AbstractConditionalLevelClassName { private set; get; }
    }
}