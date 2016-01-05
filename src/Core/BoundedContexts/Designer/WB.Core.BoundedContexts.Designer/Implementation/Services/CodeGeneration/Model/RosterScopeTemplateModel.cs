using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class RosterScopeTemplateModel
    {
        public RosterScopeTemplateModel(
            string rosterScopeType, 
            List<QuestionTemplateModel> questions, 
            List<GroupTemplateModel> groups, 
            List<RosterTemplateModel> rosters, 
            List<RosterTemplateModel> rostersInScope, 
            List<ConditionMethodAndState> conditionMethodsSortedByExecutionOrder)
        {
            GeneratedTypeName = rosterScopeType;
            Questions = questions;
            Groups = groups;
            Rosters = rosters;
            ConditionMethodsSortedByExecutionOrder = conditionMethodsSortedByExecutionOrder;
            RostersInScope = rostersInScope;
            ParentTypeName = rostersInScope[0].ParentGeneratedTypeName;
        }

        public string GeneratedTypeName { set; get; }

        public List<QuestionTemplateModel> Questions { get; set; }

        public List<GroupTemplateModel> Groups { get; set; }

        public List<RosterTemplateModel> Rosters { get; set; }

        public List<ConditionMethodAndState> ConditionMethodsSortedByExecutionOrder { get; set; }

        public IEnumerable<TypeAndNameModel> AllParentsQuestionsToTop  { set; get; }

        public IEnumerable<TypeAndNameModel> AllParentsRostersToTop { set; get; }

        public string ParentTypeName { set; get; }

        public List<RosterTemplateModel> RostersInScope { set; get; }
    }
}