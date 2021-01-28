using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models
{
    public class ExpressionStorageModel
    {
        public ExpressionStorageModel(Guid id, string className, int targetVersion)
        {
            Id = id;
            ClassName = className;
            TargetVersion = targetVersion;
        }

        public Guid Id { set; get; }

        public string ClassName { get; set; }

        public List<LevelModel> Levels { get; } = new List<LevelModel>();

        public List<ConditionMethodModel> ExpressionMethodModel { get; } = new List<ConditionMethodModel>();

        public List<OptionsFilterMethodModel> CategoricalOptionsFilterModel { get; } = new List<OptionsFilterMethodModel>();

        public List<LinkedFilterMethodModel> LinkedFilterMethodModel { get; } = new List<LinkedFilterMethodModel>();

        public List<ConditionMethodModel> VariableMethodModel { get; } = new List<ConditionMethodModel>();

        public List<LookupTableTemplateModel> LookupTables { get; set; } = new List<LookupTableTemplateModel>();
       
        public Dictionary<Guid, string> IdMap { get; set; } = new Dictionary<Guid, string>();


        public List<RosterModel> AllRosters => this.Levels
            .SelectMany(x => x.Rosters)
            .GroupBy(x => x.Variable)
            .Select(x => x.First())
            .ToList();

        public int TargetVersion { get; set; }

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

        public IEnumerable<OptionsFilterMethodModel> GetCategoricalOptionsFilters(string className)
        {
            return this.CategoricalOptionsFilterModel.Where(x => x.ClassName == className);
        }

        public IEnumerable<LinkedFilterMethodModel> GetLinkedFilters(string className)
        {
            return this.LinkedFilterMethodModel.Where(x => x.ClassName == className);
        }
    }
}
