using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Implementation.Services;

namespace WB.Core.SharedKernels.DataCollection.Tests.ExpressionProcessorTests
{
    internal class when_getting_identifiers_used_in_expression__a_or_colon_b_and_c_colon__ : ExpressionProcessorTestsContext
    {
        Establish context = () =>
        {
            expressionProcessor = CreateExpressionProcessor();
        };

        Because of = () =>
            result = expressionProcessor.GetIdentifiersUsedInExpression("[a] or ([b] and [c])");

        It should_return_identifiers__a____b__and__c__ = () =>
            result.ShouldContainOnly("a", "b", "c");

        private static IEnumerable<string> result;
        private static ExpressionProcessor expressionProcessor;
    }
}