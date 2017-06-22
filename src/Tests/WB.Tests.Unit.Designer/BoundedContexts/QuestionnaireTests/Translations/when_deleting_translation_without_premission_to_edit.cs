using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Translations
{
    internal class when_deleting_translation_without_premission_to_edit : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddOrUpdateTranslation(Create.Command.AddOrUpdateTranslation(questionnaireId, translationId, "", ownerId));

            deleteTranslation = Create.Command.DeleteTranslation(questionnaireId, translationId, sharedPersonId);
            BecauseOf();
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() => questionnaire.DeleteTranslation(deleteTranslation));

        [NUnit.Framework.Test] public void should_throw_questionnaire_exception () =>
            exception.ShouldBeOfExactType(typeof(QuestionnaireException));

        [NUnit.Framework.Test] public void should_throw_exception_with_type_DoesNotHavePermissionsForEdit () =>
            ((QuestionnaireException)exception).ErrorType.ShouldEqual(DomainExceptionType.DoesNotHavePermissionsForEdit);

        private static Exception exception;
        private static DeleteTranslation deleteTranslation;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid translationId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}