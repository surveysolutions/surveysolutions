using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    internal class when_updating_lookup_table_which_invalid_variable_name : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            updateLookupTable = Create.Command.UpdateLookupTable(questionnaireId, lookupTableId, responsibleId, lookupTableName: "$some,invalid%name");

            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() => questionnaire.UpdateLookupTable(updateLookupTable));

        [NUnit.Framework.Test] public void should_throw_questionnaire_exception () =>
            exception.ShouldBeOfExactType(typeof(QuestionnaireException));

        [NUnit.Framework.Test] public void should_throw_exception_with_type_LookupTableIsAbsent () =>
            ((QuestionnaireException)exception).ErrorType.ShouldEqual(DomainExceptionType.VariableNameSpecialCharacters);

        private static Exception exception;
        private static UpdateLookupTable updateLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid responsibleId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}