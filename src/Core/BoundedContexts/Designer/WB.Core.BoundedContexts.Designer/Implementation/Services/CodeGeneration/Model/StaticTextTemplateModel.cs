using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class StaticTextTemplateModel : ITemplateModel
    {
        public StaticTextTemplateModel(Guid id, string condition,
            IEnumerable<string> validationExpressions, string rosterScopeName, string parentScopeTypeName)
        {
            this.Id = id;
            this.Condition = condition;
            this.RosterScopeName = rosterScopeName;
            this.ParentScopeTypeName = parentScopeTypeName;

            this.ValidationExpressions = validationExpressions
                .Select((validationExpression, index)
                    => new ValidationExpressionModel(validationExpression, this.VariableName, index))
                .ToList();
        }

        public Guid Id { get; }

        public string Condition { get; }

        public List<ValidationExpressionModel> ValidationExpressions { get; }

        public string RosterScopeName { get; }
        public string ParentScopeTypeName { get; }

        public string VariableName => $"staticText_{this.Id.FormatGuid()}";
        public string StateName => CodeGenerator.PrivateFieldsPrefix + this.VariableName + CodeGenerator.StateSuffix;
        public string IdName => CodeGenerator.PrivateFieldsPrefix + this.VariableName + CodeGenerator.IdSuffix;
        public string ConditionMethodName => CodeGenerator.EnablementPrefix + this.VariableName;

        public bool HasAnyValidation() => this.ValidationExpressions.Any(x => !string.IsNullOrWhiteSpace(x.ValidationExpression));
    }
}