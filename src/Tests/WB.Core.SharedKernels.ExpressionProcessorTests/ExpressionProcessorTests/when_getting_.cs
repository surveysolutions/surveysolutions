using Machine.Specifications;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.SharedKernels.ExpressionProcessorTests.ExpressionProcessorTests
{
    internal class when_getting_ : ExpressionProcessorTestsContext
    {
        Establish context = () =>
        {
            expressionProcessor = CreateExpressionProcessor();
            directEvaluationResult = expressionProcessor.EvaluateBooleanExpression(expression, 
                delegate(string s)
                {
                    return null;
                });

            serializedExpression = expressionProcessor.GetSerializedExpression(expression);
        };

        Because of = () =>
            indirectEvaluationResult = expressionProcessor.EvaluateSerializedBooleanExpression(serializedExpression,
                delegate(string s)
                {
                    return null;
                });

        It should_ = () =>
            indirectEvaluationResult.Equals(directEvaluationResult);


        private static string expression = "2 or (3 == 2) or 3 or 4 != 5 and 4 == 3";

        private static string serializedExpression;
        private static bool directEvaluationResult;
        private static bool indirectEvaluationResult;
        private static IExpressionProcessor expressionProcessor;
    }
}