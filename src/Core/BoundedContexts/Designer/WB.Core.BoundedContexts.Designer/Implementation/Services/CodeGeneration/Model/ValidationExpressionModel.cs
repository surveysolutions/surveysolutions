using System.Globalization;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class ValidationExpressionModel
    {
        public ValidationExpressionModel(string validationExpression, string variableName, int order)
        {
            this.ValidationExpression = validationExpression;
            this.VariableName = variableName;
            this.Order = order;
        }

        public string ValidationExpression { get; }
        public string ValidationMethodName => $"{CodeGenerator.ValidationPrefix}{this.VariableName}__{this.Order.ToString(CultureInfo.InvariantCulture)}";
        public string VariableName { get; }
        public int Order { get; }
    }
}
