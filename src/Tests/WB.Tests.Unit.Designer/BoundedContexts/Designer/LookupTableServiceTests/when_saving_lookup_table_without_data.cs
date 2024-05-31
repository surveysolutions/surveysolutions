using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    [TestOf(typeof(LookupTableService))]
    internal class LookupTableServiceTests
    {
        [OneTimeSetUp]
        public void context()
        {
            lookupTableService = Create.LookupTableService();
        }

        [TestCase("no\trow\tcode\tcolumn\n")]
        [TestCase("a\n")]
        [TestCase("a\tb")]
        [TestCase("a\n\n\n")]
        public void when_saving_lookup_table_without_rowcode_header_should_throw_ArgumentException (string content) {

            var exception = Assert.Throws<ArgumentException>(() =>
                lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, content));

            exception.Message.Should().Be(ExceptionMessages.LookupTables_rowcode_column_is_mandatory);
        }
        
        [TestCase("exists\trowcode\tcolumn\n")]
        [TestCase("a\trowcode\n")]
        [TestCase("a\trowcode\tb")]
        [TestCase("a\trowcode\n\n\n")]
        public void when_saving_lookup_table_without_data_should_throw_ArgumentException (string content) {

            var exception = Assert.Throws<ArgumentException>(() =>
                lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, content));

            exception.Message.Should().Be(ExceptionMessages.LookupTables_cant_has_empty_content);
        }

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static LookupTableService lookupTableService;
    }
}
