using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    [Ignore("not removing but creating a new")]
    internal class when_updating_lookup_table_which_doesnt_exists : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            updateLookupTable = Create.Command.UpdateLookupTable(questionnaireId, lookupTableId, responsibleId);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            exception = Catch.Exception(() => questionnaire.UpdateLookupTable(updateLookupTable));

        It should_throw_questionnaire_exception = () =>
            exception.ShouldBeOfExactType(typeof(QuestionnaireException));

        It should_throw_exception_with_type_LookupTableIsAbsent = () =>
            ((QuestionnaireException)exception).ErrorType.ShouldEqual(DomainExceptionType.LookupTableIsAbsent);

        private static Exception exception;
        private static UpdateLookupTable updateLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}