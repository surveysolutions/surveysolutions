using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class RosterTemplateModel : RosterScopeBaseModel
    {
        public RosterTemplateModel()
        {
            this.LinkedQuestionFilterExpressions = new List<LinkedQuestionFilterExpressionModel>();
        }

        public string Conditions { set; get; }

        public Guid Id { set; get; }

        public string VariableName { set; get; }
        public string ParentScopeTypeName { get; set; }

        public string StateName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.StateSuffix;
        public string IdName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.IdSuffix;
        public string ConditionsMethodName => CodeGenerator.EnablementPrefix + VariableName;

        public List<LinkedQuestionFilterExpressionModel> LinkedQuestionFilterExpressions { get; set; }
    }
}