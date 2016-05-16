using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class RosterScopeTemplateModel
    {
        public RosterScopeTemplateModel(
            string typeName, 
            List<QuestionTemplateModel> questions, 
            List<StaticTextTemplateModel> staticTexts, 
            List<GroupTemplateModel> groups, 
            List<RosterTemplateModel> rosters, 
            List<RosterTemplateModel> rostersInScope, 
            List<ConditionMethodAndState> conditionMethodsSortedByExecutionOrder,
            List<LinkedQuestionFilterExpressionModel> linkedQuestionFilterExpressions,
            List<VariableTemplateModel> variables)
        {
            TypeName = typeName;
            Questions = questions;
            StaticTexts = staticTexts;
            Groups = groups;
            Rosters = rosters;
            ConditionMethodsSortedByExecutionOrder = conditionMethodsSortedByExecutionOrder;
            RostersInScope = rostersInScope;
            ParentTypeName = rostersInScope[0].ParentTypeName;
            LinkedQuestionFilterExpressions = linkedQuestionFilterExpressions;
            Variables = variables;
        }

        public string TypeName { set; get; }

        public List<QuestionTemplateModel> Questions { get; set; }

        public List<StaticTextTemplateModel> StaticTexts { get; set; }

        public List<GroupTemplateModel> Groups { get; set; }

        public List<RosterTemplateModel> Rosters { get; set; }

        public List<VariableTemplateModel> Variables { get; set; }

        public List<ConditionMethodAndState> ConditionMethodsSortedByExecutionOrder { get; set; }

        public IEnumerable<HierarchyReferenceModel> AllParentsQuestionsToTop  { set; get; }

        public IEnumerable<HierarchyReferenceModel> AllParentsRostersToTop { set; get; }

        public string ParentTypeName { set; get; }

        public List<RosterTemplateModel> RostersInScope { set; get; }

        public List<LinkedQuestionFilterExpressionModel> LinkedQuestionFilterExpressions { get; set; }
    }
}