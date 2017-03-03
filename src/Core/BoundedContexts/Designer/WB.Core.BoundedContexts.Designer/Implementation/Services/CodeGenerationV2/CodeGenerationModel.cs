using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Util;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public class CodeGenerationModel
    {
        public CodeGenerationModel()
        {
            this.AllQuestions = new List<QuestionModel>();
            this.AllLevels = new List<LevelModel>();
        }
        public Guid Id { set; get; }
        public List<QuestionModel> AllQuestions { set; get; }
        public List<LevelModel> AllLevels { set; get; }

        public List<StaticTextModel> AllStaticTexts { get; private set; } = new List<StaticTextModel>();

        // change it later
        public List<RosterModel> AllRosters => AllLevels
            .SelectMany(x => x.Rosters)
            .GroupBy(x => x.Variable)
            .Select(x => x.First())
            .ToList();

        public List<ConditionMethodModel> ExpressionMethodModel { private set;  get; } = new List<ConditionMethodModel>();

        public List<OptionsFilterMethodModel> CategoricalOptionsFilterModel { private set;  get; } = new List<OptionsFilterMethodModel>();

        public List<LinkedFilterMethodModel> LinkedFilterMethodModel { private set;  get; } = new List<LinkedFilterMethodModel>();

        public List<Guid> ConditionsPlayOrder { get; set; }

        public string[] AdditionalInterfaces { get; set; }
        public string[] Namespaces { get; set; }

        public List<LookupTableTemplateModel> LookupTables { get; set; }
        public string ClassName { get; set; }
        

        public QuestionModel GetQuestionById(Guid questionId)
        {
            return AllQuestions.FirstOrDefault(x => x.Id == questionId);
        }

        public LevelModel GetLevelByVariable(string variable)
        {
            return AllRosters.FirstOrDefault(x => x.Variable == variable)?.Level;
        }

        public string GetClassNameByRosterScope(RosterScope rosterScope)
        {
            return AllLevels.First(x => x.RosterScope.Equals(rosterScope)).ClassName;
        }

        public IEnumerable<ConditionMethodModel> GetEnablementConditions(string className)
        {
            return ExpressionMethodModel
                .Where(x => x.Location.ExpressionType == ExpressionLocationType.Condition)
                .Where(x => x.ClassName == className);
        }

        public Dictionary<string, string[]> GetValidationConditions(string className)
        {
            return ExpressionMethodModel
                .Where(x => x.Location.ExpressionType == ExpressionLocationType.Validation)
                .Where(x => x.ClassName == className)
                .GroupBy(x => x.Variable)
                .ToDictionary(x => x.Key, x => x.OrderBy(v => v.Location.ExpressionPosition).Select(v => v.MethodName).ToArray());
        }
        
    }
}