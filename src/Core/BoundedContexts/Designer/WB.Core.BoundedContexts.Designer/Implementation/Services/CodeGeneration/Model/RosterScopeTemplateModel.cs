using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class RosterScopeTemplateModel
    {
        public RosterScopeTemplateModel(
            string typeName, 
            List<QuestionTemplateModel> questions, 
            List<GroupTemplateModel> groups, 
            List<RosterTemplateModel> rosters, 
            List<RosterTemplateModel> rostersInScope, 
            List<ConditionMethodAndState> conditionMethodsSortedByExecutionOrder,
            List<LinkedQuestionFilterExpressionModel> linkedQuestionFilterExpressions)
        {
            TypeName = typeName;
            Questions = questions;
            Groups = groups;
            Rosters = rosters;
            ConditionMethodsSortedByExecutionOrder = conditionMethodsSortedByExecutionOrder;
            RostersInScope = rostersInScope;
            ParentTypeName = rostersInScope[0].ParentTypeName;
            LinkedQuestionFilterExpressions = linkedQuestionFilterExpressions;
        }

        public string TypeName { set; get; }

        public List<QuestionTemplateModel> Questions { get; set; }

        public List<GroupTemplateModel> Groups { get; set; }

        public List<RosterTemplateModel> Rosters { get; set; }

        public List<ConditionMethodAndState> ConditionMethodsSortedByExecutionOrder { get; set; }

        public IEnumerable<HierarchyReferenceModel> AllParentsQuestionsToTop  { set; get; }

        public IEnumerable<HierarchyReferenceModel> AllParentsRostersToTop { set; get; }

        public string ParentTypeName { set; get; }

        public List<RosterTemplateModel> RostersInScope { set; get; }

        public List<LinkedQuestionFilterExpressionModel> LinkedQuestionFilterExpressions { get; set; }
    }
}