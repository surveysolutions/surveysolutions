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

        public string ValidationExpression { private set; get; }
        public string ValidationMethodName => string.Format("{0}{1}__{2}", CodeGenerator.ValidationPrefix, VariableName, this.Order.ToString(CultureInfo.InvariantCulture)) ;
        public string VariableName { set; get; }
        public int Order { private set; get; }
    }
}
