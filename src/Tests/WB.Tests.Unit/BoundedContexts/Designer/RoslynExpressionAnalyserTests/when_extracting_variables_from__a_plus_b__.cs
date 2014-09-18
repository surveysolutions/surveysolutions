using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;

namespace WB.Tests.Unit.BoundedContexts.Designer.RoslynExpressionAnalyserTests
{
    internal class when_extracting_variables_from__a_plus_b__ : RoslynExpressionAnalyserTestsContext
    {
        Establish context = () =>
        {
            analyzer = Create.RoslynExpressionAnalyser();
        };

        Because of = () =>
            result = analyzer.ExtractVariables("a+b").ToList();

        It should_return_a_and_b = () =>
            result.ShouldContainOnly("a", "b");

        private static IEnumerable<string> result;
        private static RoslynExpressionAnalyser analyzer;
    }
}