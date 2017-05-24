using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
{
    public class ExpressionStorageModel
    {
        public ExpressionStorageModel()
        {
            this.AllQuestions = new List<QuestionModel>();
            this.AllLevels = new List<LevelModel>();
            this.AllVariables = new List<VariableModel>();
        }
        public Guid Id { set; get; }
        public List<QuestionModel> AllQuestions { set; get; }
        public List<LevelModel> AllLevels { set; get; }

        public List<VariableModel> AllVariables { get; set; }

        public List<StaticTextModel> AllStaticTexts { get; private set; } = new List<StaticTextModel>();

        // change it later
        public List<RosterModel> AllRosters => this.AllLevels
            .SelectMany(x => x.Rosters)
            .GroupBy(x => x.Variable)
            .Select(x => x.First())
            .ToList();

        public List<ConditionMethodModel> ExpressionMethodModel { private set;  get; } = new List<ConditionMethodModel>();

        public List<OptionsFilterMethodModel> CategoricalOptionsFilterModel { private set;  get; } = new List<OptionsFilterMethodModel>();

        public List<LinkedFilterMethodModel> LinkedFilterMethodModel { private set;  get; } = new List<LinkedFilterMethodModel>();

        public List<ConditionMethodModel> VariableMethodModel { private set; get; } = new List<ConditionMethodModel>();

        public string[] AdditionalInterfaces { get; set; }
        public string[] Namespaces { get; set; }

        public List<LookupTableTemplateModel> LookupTables { get; set; }
        public string ClassName { get; set; }
        public Dictionary<Guid, string> IdMap { get; set; }

        public QuestionModel GetQuestionById(Guid questionId)
        {
            return this.AllQuestions.FirstOrDefault(x => x.Id == questionId);
        }

        public LevelModel GetLevelByVariable(string variable)
        {
            return this.AllRosters.FirstOrDefault(x => x.Variable == variable)?.Level;
        }

        public string GetClassNameByRosterScope(RosterScope rosterScope)
        {
            return this.AllLevels.First(x => x.RosterScope.Equals(rosterScope)).ClassName;
        }

        public IEnumerable<ConditionMethodModel> GetEnablementConditions(string className)
        {
            return this.ExpressionMethodModel
                .Where(x => x.Location.ExpressionType == ExpressionLocationType.Condition)
                .Where(x => x.ClassName == className);
        }

        public Dictionary<string, string[]> GetValidationConditions(string className)
        {
            return this.ExpressionMethodModel
                .Where(x => x.Location.ExpressionType == ExpressionLocationType.Validation)
                .Where(x => x.ClassName == className)
                .GroupBy(x => x.Variable)
                .ToDictionary(x => x.Key, x => x.OrderBy(v => v.Location.ExpressionPosition).Select(v => v.MethodName).ToArray());
        }

        public IEnumerable<ConditionMethodModel> GetVariableExpressions(string className)
        {
            return this.VariableMethodModel
               .Where(x => x.Location.ItemType == ExpressionLocationItemType.Variable)
               .Where(x => x.ClassName == className);
        }

        public VariableModel GetVariableById(Guid variableId)
        {
            return this.AllVariables.FirstOrDefault(x => x.Id == variableId);
        }
    }
}