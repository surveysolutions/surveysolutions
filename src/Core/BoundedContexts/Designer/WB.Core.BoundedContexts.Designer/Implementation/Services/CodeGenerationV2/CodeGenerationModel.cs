using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public class CodeGenerationModel
    {
        public CodeGenerationModel()
        {
            this.AllQuestions = new List<QuestionTemplateModel>();
            this.AllStaticTexts = new List<StaticTextTemplateModel>();
            this.AllGroups = new List<GroupTemplateModel>();
            this.AllRosters = new List<RosterTemplateModel>();
            this.AllLinkedQuestionFilters = new List<LinkedQuestionFilterExpressionModel>();
            this.AllVariables = new List<VariableTemplateModel>();
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
        public Dictionary<Guid, List<Guid>> RosterDependencies { get; set; }
        public Dictionary<Guid, Guid> LinkedQuestionByRosterDependencies { get; set; }

        public List<Guid> ConditionsPlayOrder { get; set; }

        public string[] AdditionalInterfaces { get; set; }
        public string[] Namespaces { get; set; }

        public List<LookupTableTemplateModel> LookupTables { get; set; }
        public Dictionary<string, ConditionDescriptionModel> MethodModels { get; set; }

        public Dictionary<string, OptionsFilterConditionDescriptionModel> CategoricalOptionsFilterModels { get; set; }
        public Dictionary<string, LinkedFilterConditionDescriptionModel> LinkedFilterModels { get; set; }

        public List<LinkedQuestionFilterExpressionModel> AllLinkedQuestionFilters { get; set; }
    }
}