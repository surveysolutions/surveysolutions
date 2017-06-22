using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.MacrosSubstitutionServiceTests
{
    internal class when_inlining_macroses_in_expression
    {
        [NUnit.Framework.OneTimeSetUp] public void context ()
        {
            macros = new List<Macro>
            {
                Create.Macro("a", "invalid"),
                Create.Macro("aa", "aaexpression"),
                Create.Macro("b", "invalid"),
                Create.Macro("bb", "bbexpression"),
            };
            expression = "$aa + $bb";
            macrosSubstitutionService = Create.MacrosSubstitutionService();
            BecauseOf();
        }

        private void BecauseOf() =>
            result = macrosSubstitutionService.InlineMacros(expression, macros);

        [NUnit.Framework.Test] public void should_inline_longer_macros_first () =>
            result.ShouldEqual("aaexpression + bbexpression");

        private static List<Macro> macros; 
        private static string expression;
        private static string result;
        private static MacrosSubstitutionService macrosSubstitutionService;
    }
}
