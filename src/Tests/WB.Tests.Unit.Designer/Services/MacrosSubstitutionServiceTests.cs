#nullable enable
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.Designer.Services
{
    [TestFixture]
    [TestOf(typeof(MacrosSubstitutionService))]
    public class MacrosSubstitutionServiceTests
    {
        private readonly MacrosSubstitutionService service = new MacrosSubstitutionService();

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void when_expression_is_empty_should_return_empty_string(string expression)
        {
            service.InlineMacros(expression, new[] { new Macro { Name = "m", Content = "1" } })
                .Should().BeEmpty();
        }

        [Test]
        public void when_expression_has_no_macro_marker_should_return_it_unchanged()
        {
            service.InlineMacros("a > 1", new[] { new Macro { Name = "a", Content = "2" } })
                .Should().Be("a > 1");
        }

        [Test]
        public void when_expression_contains_macro_should_replace_it_with_content()
        {
            service.InlineMacros("$age > 18", new[] { new Macro { Name = "age", Content = "self.Age" } })
                .Should().Be("self.Age > 18");
        }

        [Test]
        public void when_macro_names_share_prefix_should_replace_longest_name_first()
        {
            var macros = new List<Macro>
            {
                new Macro { Name = "m", Content = "SHORT" },
                new Macro { Name = "m1", Content = "LONG" },
            };

            service.InlineMacros("$m1 && $m", macros).Should().Be("LONG && SHORT");
        }

        [Test]
        public void when_macro_is_unknown_should_leave_marker_in_place()
        {
            service.InlineMacros("$unknown + 1", new List<Macro>()).Should().Be("$unknown + 1");
        }
    }
}

