using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionnaireExpressionStateModel
    {
        public QuestionnaireExpressionStateModel(string className,Guid id, QuestionnaireLevelTemplateModel questionnaireLevelModel, 
            Dictionary<Guid, List<Guid>> conditionalDependencies, Dictionary<Guid, List<Guid>> structuralDependencies, 
            Dictionary<Guid, List<Guid>> rosterDependencies,  string[] additionalInterfaces, string[] namespaces, 
            List<LookupTableTemplateModel> lookupTables)
        {
            AllQuestions = new List<QuestionTemplateModel>();
            AllStaticTexts = new List<StaticTextTemplateModel>();
            AllGroups = new List<GroupTemplateModel>();
            AllRosters = new List<RosterTemplateModel>();
            AllLinkedQuestionFilters = new List<LinkedQuestionFilterExpressionModel>();
            AllVariables = new List<VariableTemplateModel>();

            RostersGroupedByScope = new Dictionary<string, RosterScopeTemplateModel>();
            LinkedQuestionByRosterDependencies = new Dictionary<Guid, Guid>();
            ConditionsPlayOrder = new List<Guid>();
            MethodModels = new Dictionary<string, ConditionDescriptionModel>();
            CategoricalOptionsFilterModels = new Dictionary<string, OptionsFilterConditionDescriptionModel>();
            LinkedFilterModels = new Dictionary<string, LinkedFilterConditionDescriptionModel>();

            ClassName = className;
            Id = id;
            QuestionnaireLevelModel = questionnaireLevelModel;
            ConditionalDependencies = conditionalDependencies;
            StructuralDependencies = structuralDependencies;
            RosterDependencies = rosterDependencies;
            AdditionalInterfaces = additionalInterfaces;
            Namespaces = namespaces;
            LookupTables = lookupTables;

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

        public List<LinkedQuestionFilterExpressionModel> AllLinkedQuestionFilters {get;set;}
    }
}
