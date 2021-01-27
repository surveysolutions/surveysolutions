using System;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_without_15001_rows
    {
        [NUnit.Framework.Test]
        public void should_throw_ArgumentException()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"no{_}rowcode{_}column{_end}");
            for (int i = 0; i < 500_004; i++)
            {
                sb.AppendLine($"1{_}2{_}3{_end}");
            }

            lookupTableService = Create.LookupTableService();
            exception = Assert.Throws<ArgumentException>(() =>
                lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, sb.ToString()));

            exception.Message.Should().Be(
                string.Format(ExceptionMessages.LookupTables_too_many_rows, $"{500_000:n0}", $"{500_004:n0}"));
        }

        private static Exception exception;

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        
        private static LookupTableService lookupTableService;

        private static string _ = "\t";
        private static string _end = "\n";
    }
}
