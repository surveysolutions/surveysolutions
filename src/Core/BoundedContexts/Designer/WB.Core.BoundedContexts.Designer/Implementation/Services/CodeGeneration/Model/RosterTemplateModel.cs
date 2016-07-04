using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class RosterTemplateModel : RosterScopeBaseModel
    {
        public RosterTemplateModel(
            string rosterScopeName, 
            string typeName, 
            List<Guid> rosterScope, 
            string parentTypeName, 
            string conditions, 
            Guid id, 
            string variableName, 
            string parentScopeTypeName,
            List<LinkedQuestionFilterExpressionModel> linkedQuestionFilterExpressions)
            : base(
                rosterScopeName, 
                typeName, 
                new List<GroupTemplateModel>(), 
                new List<QuestionTemplateModel>(), 
                new List<StaticTextTemplateModel>(), 
                new List<RosterTemplateModel>(), 
                rosterScope, 
                new List<VariableTemplateModel>(), 
                parentTypeName)
        {
            this.Conditions = conditions;
            this.Id = id;
            this.VariableName = variableName;
            this.ParentScopeTypeName = parentScopeTypeName;
            this.LinkedQuestionFilterExpressions = linkedQuestionFilterExpressions;
            this.LinkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter = new List<LinkedQuestionVerifierModel>();
        }

        public string Conditions { get; }
        public Guid Id { get; }
        public string VariableName { get; }
        public string ParentScopeTypeName { get; }
        public List<LinkedQuestionFilterExpressionModel> LinkedQuestionFilterExpressions { get; }

        public string StateName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.StateSuffix;
        public string IdName => CodeGenerator.PrivateFieldsPrefix + VariableName + CodeGenerator.IdSuffix;
        public string ConditionsMethodName => CodeGenerator.EnablementPrefix + VariableName;
        public List<LinkedQuestionVerifierModel> LinkedQuestionsThatReferencesRosterDependentOnQuestionWithOptionsFilter { get; set; }
    }
}