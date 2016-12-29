using System.Collections.Generic;
using NHibernate.Util;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class ConditionDescriptionModel
    {
        public ConditionDescriptionModel(
            string className, 
            string methodName, 
            string[] namespaces,
            string expression, 
            bool generateSelf, 
            string variableName,
            string returnType = "bool")
        {
            this.ClassName = className;
            this.MethodName = methodName;
            this.Namespaces = namespaces;
            this.Expression = expression;
            this.VariableName = variableName;
            this.GenerateSelf = generateSelf;
            this.ReturnType = returnType;
        }

        public string ClassName { set; get; }
        public string MethodName { set; get; }
        public string[] Namespaces { get; set; }
        public string Expression { set; get; }
        public string VariableName { set; get; }
        public bool GenerateSelf { set; get; }
        public string ReturnType { get; set; }
        public List<VariableModel> AdditionalVariables { get; set; } = new List<VariableModel>();
    }

    public class OptionsFilterConditionDescriptionModel : ConditionDescriptionModel
    {
        public OptionsFilterConditionDescriptionModel(string className, string methodName, string[] namespaces, string expression, string variableName) 
            : base(className, methodName, namespaces, expression, true, variableName, "bool")
        {
        }
    }

    public class LinkedFilterConditionDescriptionModel : ConditionDescriptionModel
    {
        public string LinkedQuestionScopeName { get; }

        public bool IsSourceAndLinkedQuestionOnSameLevel => LinkedQuestionScopeName == this.ClassName;

        public LinkedFilterConditionDescriptionModel(string className, string methodName, string[] namespaces, string expression, string linkedQuestionScopeName)
            : base(className, methodName, namespaces, expression, false, string.Empty, "bool")
        {
            this.LinkedQuestionScopeName = linkedQuestionScopeName;
        }
    }
}
