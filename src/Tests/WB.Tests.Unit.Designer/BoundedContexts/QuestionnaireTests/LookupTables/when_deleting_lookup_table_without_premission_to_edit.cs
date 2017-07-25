using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    internal class when_deleting_lookup_table_without_premission_to_edit : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddLookupTable(Create.Command.AddLookupTable(questionnaireId, lookupTableId, ownerId));
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.View, ownerId);

            deleteLookupTable = Create.Command.DeleteLookupTable(questionnaireId, lookupTableId, sharedPersonId);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() => questionnaire.DeleteLookupTable(deleteLookupTable));

        [NUnit.Framework.Test] public void should_throw_exception () =>
            exception.ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_throw_questionnaire_exception () =>
            exception.ShouldBeOfExactType(typeof(QuestionnaireException));

        [NUnit.Framework.Test] public void should_throw_exception_with_type_DoesNotHavePermissionsForEdit () =>
            ((QuestionnaireException)exception).ErrorType.ShouldEqual(DomainExceptionType.DoesNotHavePermissionsForEdit);

        private static Exception exception;
        private static DeleteLookupTable deleteLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}