using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    internal class when_updating_lookup_table_without_premission_to_edit : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_questionnaire_exception () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddLookupTable(Create.Command.AddLookupTable(questionnaireId, lookupTableId, ownerId));
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.View, ownerId);

            updateLookupTable = Create.Command.UpdateLookupTable(questionnaireId, lookupTableId, sharedPersonId);

            var exception = Assert.Throws<QuestionnaireException>(() => questionnaire.UpdateLookupTable(updateLookupTable));

            exception.ErrorType.Should().Be(DomainExceptionType.DoesNotHavePermissionsForEdit);
        }

        private static UpdateLookupTable updateLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid lookupTableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
