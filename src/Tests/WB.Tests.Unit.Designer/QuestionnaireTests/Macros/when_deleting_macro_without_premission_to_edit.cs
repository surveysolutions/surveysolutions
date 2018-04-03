using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Macros
{
    internal class when_deleting_macro_without_premission_to_edit : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_questionnaire_exception() {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddMacro(Create.Command.AddMacro(questionnaireId, macroId, ownerId));
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.View, ownerId);

            deleteMacro = Create.Command.DeleteMacro(questionnaireId, macroId, sharedPersonId);

            var exception = Assert.Throws<QuestionnaireException>(() => questionnaire.DeleteMacro(deleteMacro));
            exception.ErrorType.Should().Be(DomainExceptionType.DoesNotHavePermissionsForEdit);
        }

        private static DeleteMacro deleteMacro;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
