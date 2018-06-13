using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    internal class when_deleting_lookup_table_which_doesnt_exists : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_exception_with_type_LookupTableIsAbsent () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            deleteLookupTable = Create.Command.DeleteLookupTable(questionnaireId, lookupTableId, responsibleId);

            var exception = Assert.Throws<QuestionnaireException>(() => questionnaire.DeleteLookupTable(deleteLookupTable));
            exception.ErrorType.Should().Be(DomainExceptionType.LookupTableIsAbsent);
        }

        private static DeleteLookupTable deleteLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
