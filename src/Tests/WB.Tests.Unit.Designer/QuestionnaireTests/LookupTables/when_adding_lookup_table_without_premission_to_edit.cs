using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.LookupTables
{
    internal class when_adding_lookup_table_without_premission_to_edit : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_exception () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.View, ownerId);

            addLookupTable = Create.Command.AddLookupTable(questionnaireId, macroId, sharedPersonId);
            var exception = Assert.Throws<QuestionnaireException>(() => questionnaire.AddLookupTable(addLookupTable));

            exception.ErrorType.Should().Be(DomainExceptionType.DoesNotHavePermissionsForEdit);
        }

        private static AddLookupTable addLookupTable;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
