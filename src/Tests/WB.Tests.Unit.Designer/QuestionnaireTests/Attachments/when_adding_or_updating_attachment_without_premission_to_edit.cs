using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Attachments
{
    internal class when_adding_or_updating_attachment_without_premission_to_edit : QuestionnaireTestsContext
    {
        [NUnit.Framework.Test]
        public void should_throw_exception_with_type_DoesNotHavePermissionsForEdit()
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.View, ownerId);
            addOrUpdateAttachment = Create.Command.AddOrUpdateAttachment(questionnaireId, attachmentId, "", sharedPersonId, "");

            var exception = Assert.Throws<QuestionnaireException>(() => questionnaire.AddOrUpdateAttachment(addOrUpdateAttachment));

            exception.ErrorType.Should().Be(DomainExceptionType.DoesNotHavePermissionsForEdit);
        }

        private static AddOrUpdateAttachment addOrUpdateAttachment;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}
