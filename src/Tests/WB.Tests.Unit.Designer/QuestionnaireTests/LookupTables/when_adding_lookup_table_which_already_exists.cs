using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    internal class when_adding_lookup_table_which_already_exists : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_exception_with_type_LookupTableAlreadyExist () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            questionnaire.AddLookupTable(Create.Command.AddLookupTable(questionnaireId, lookupTableId, responsibleId));

            addLookupTable = Create.Command.AddLookupTable(questionnaireId, lookupTableId, responsibleId);

            eventContext = new EventContext();

            var exception = Assert.Throws<QuestionnaireException>(() => questionnaire.AddLookupTable(addLookupTable));

            exception.ErrorType.Should().Be(DomainExceptionType.LookupTableAlreadyExist);
        }

        private static AddLookupTable addLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}
