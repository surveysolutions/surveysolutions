using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_without_data
    {

        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            lookupTableService = Create.LookupTableService();
        }

        
        [TestCase("no\trow\tcode\tcolumn\n")]
        [TestCase("a\n")]
        [TestCase("a\tb")]
        [TestCase("a\n\n\n")]
        [NUnit.Framework.Test] public void should_throw_ArgumentException (string content) {
            fileContent = content;

            exception = Assert.Throws<ArgumentException>(() =>
                lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, fileContent));

            exception.Message.Should().Be(ExceptionMessages.LookupTables_cant_has_empty_content);
        }

        private static Exception exception;

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string fileContent;
        private static LookupTableService lookupTableService;
    }
}
