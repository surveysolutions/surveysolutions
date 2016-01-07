using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class QuestionnaireExpressionStateModel
    {
        public Guid Id { set; get; }
        public List<QuestionTemplateModel> AllQuestions { set; get; }
        public List<GroupTemplateModel> AllGroups { set; get; }
        public List<RosterTemplateModel> AllRosters { set; get; }
        public string ClassName { set; get; }
        public Dictionary<string, RosterScopeTemplateModel> RostersGroupedByScope { set; get; }
        public QuestionnaireLevelTemplateModel QuestionnaireLevelModel { set; get; }

        public Dictionary<Guid, List<Guid>> ConditionalDependencies { set; get; }
        public Dictionary<Guid, List<Guid>> StructuralDependencies { set; get; }
        public List<Guid> ConditionsPlayOrder { get; set; }

        public string[] AdditionInterfaces { get; set; }
        public string[] Namespaces { get; set; }

        public List<LookupTableTemplateModel> LookupTables { get; set; }
        public Dictionary<string, ExpressionMethodModel> MethodModels { get; set; }
    }
}