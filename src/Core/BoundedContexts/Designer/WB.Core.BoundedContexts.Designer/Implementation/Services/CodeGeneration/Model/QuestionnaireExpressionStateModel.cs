using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionnaireExpressionStateModel
    {
        public QuestionnaireExpressionStateModel()
        {
            AllQuestions = new List<QuestionTemplateModel>();
            AllStaticTexts = new List<StaticTextTemplateModel>();
            AllGroups = new List<GroupTemplateModel>();
            AllRosters = new List<RosterTemplateModel>();
            AllLinkedQuestionFilters = new List<LinkedQuestionFilterExpressionModel>();
            AllVariables=new List<VariableTemplateModel>();
        }
        public Guid Id { set; get; }
        public List<QuestionTemplateModel> AllQuestions { set; get; }
        public List<VariableTemplateModel> AllVariables { get; }
        public List<StaticTextTemplateModel> AllStaticTexts { get; }
        public List<GroupTemplateModel> AllGroups { set; get; }
        public List<RosterTemplateModel> AllRosters { set; get; }
        public string ClassName { set; get; }
        public Dictionary<string, RosterScopeTemplateModel> RostersGroupedByScope { set; get; }
        public QuestionnaireLevelTemplateModel QuestionnaireLevelModel { set; get; }

        public Dictionary<Guid, List<Guid>> ConditionalDependencies { set; get; }
        public Dictionary<Guid, List<Guid>> StructuralDependencies { set; get; }
        public List<Guid> ConditionsPlayOrder { get; set; }

        public string[] AdditionalInterfaces { get; set; }
        public string[] Namespaces { get; set; }

        public List<LookupTableTemplateModel> LookupTables { get; set; }
        public Dictionary<string, ConditionDescriptionModel> MethodModels { get; set; }

        public Dictionary<string, OptionsFilterConditionDescriptionModel> CategoricalOptionsFilterModels { get; set; }

        public List<LinkedQuestionFilterExpressionModel> AllLinkedQuestionFilters {get;set;}
    }
}