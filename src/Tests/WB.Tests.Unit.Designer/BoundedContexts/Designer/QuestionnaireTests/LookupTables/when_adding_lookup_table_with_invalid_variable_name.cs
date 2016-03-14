using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests.LookupTables
{
    internal class when_adding_lookup_table_with_invalid_variable_name : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            questionnaire.AddLookupTable(Create.Command.AddLookupTable(questionnaireId, lookupTableId, responsibleId));

            addLookupTable = Create.Command.AddLookupTable(questionnaireId, lookupTableId, responsibleId, lookupTableName: "if");

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            exception = Catch.Exception(() => questionnaire.AddLookupTable(addLookupTable));

        It should_throw_questionnaire_exception = () =>
            exception.ShouldBeOfExactType(typeof(QuestionnaireException));

        It should_throw_exception_with_type_LookupTableAlreadyExist = () =>
            ((QuestionnaireException)exception).ErrorType.ShouldEqual(DomainExceptionType.VariableNameShouldNotMatchWithKeywords);

        private static Exception exception;
        private static AddLookupTable addLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static EventContext eventContext;
    }
}