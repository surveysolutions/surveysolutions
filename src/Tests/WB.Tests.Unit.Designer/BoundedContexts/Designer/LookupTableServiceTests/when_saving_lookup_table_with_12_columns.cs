using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.LookupTableServiceTests
{
    internal class when_saving_lookup_table_with_12_columns
    {
        [NUnit.Framework.Test] public void should_throw_ArgumentException () {
            fileContent =
                $"header1{_}header2{_}header3{_}header4{_}header5{_}header6{_}header7{_}header8{_}header9{_}header10{_}header11{_}header12{_end}" +
                $"1{_}2{_}3{_}4{_}5{_}6{_}7{_}8{_}9{_}10{_}11{_}12{_end}";
        
            lookupTableService = Create.LookupTableService();
            exception = Assert.Throws<ArgumentException>(() => 
                lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, fileContent));

            exception.Message.Should().Be(string.Format(ExceptionMessages.LookupTables_too_many_columns, 11));
        }

        private static Exception exception;

        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string fileContent;
        private static LookupTableService lookupTableService;

        private static string _ = "\t";
        private static string _end = "\n";
    }
}
