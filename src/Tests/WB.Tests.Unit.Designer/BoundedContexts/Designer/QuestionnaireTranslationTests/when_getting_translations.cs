using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTranslationTests
{
    [Subject(typeof(QuestionnaireTranslation))]
    internal class when_getting_translations : QuestionnaireTranslationTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            storedTranslations = new List<TranslationDto>
            {
                Create.TranslationDto(type: TranslationType.Title,
                    translation: "title",
                    translationId: translationId,
                    questionnaireEntityId: questionId),
                Create.TranslationDto(type: TranslationType.Instruction,
                    translation: "instruction",
                    translationId: translationId,
                    questionnaireEntityId: questionId),
                Create.TranslationDto(type: TranslationType.OptionTitle,
                    translation: "option1",
                    translationId: translationId,
                    questionnaireEntityId: questionId,
                    translationIndex: "1"),
                Create.TranslationDto(type: TranslationType.ValidationMessage,
                    translation: "validation message",
                    translationId: translationId,
                    questionnaireEntityId: questionId,
                    translationIndex: "1")
            };
            BecauseOf();
        }

        private void BecauseOf() => translation = CreateQuestionnaireTranslation(storedTranslations);

        [NUnit.Framework.Test] public void should_return_translated_title () => translation.GetTitle(questionId).ShouldEqual("title");

        [NUnit.Framework.Test] public void should_return_translated_instruction () => translation.GetInstruction(questionId).ShouldEqual("instruction");

        [NUnit.Framework.Test] public void should_return_translated_option () => translation.GetAnswerOption(questionId, "1").ShouldEqual("option1");

        [NUnit.Framework.Test] public void should_return_translated_validation () => translation.GetValidationMessage(questionId, 1).ShouldEqual("validation message");

        [NUnit.Framework.Test] public void should_return_null_for_missing_title () => translation.GetTitle(Guid.NewGuid()).ShouldBeNull();

        [NUnit.Framework.Test] public void should_return_null_for_missing_instruction () => translation.GetInstruction(Guid.NewGuid()).ShouldBeNull();

        [NUnit.Framework.Test] public void should_return_null_for_missing_translated_option () => translation.GetAnswerOption(Guid.NewGuid(), "1").ShouldBeNull();

        [NUnit.Framework.Test] public void should_return_null_for_missing_translated_validation () => translation.GetValidationMessage(Guid.NewGuid(), 1).ShouldBeNull();

        static ITranslation translation;
        static List<TranslationDto> storedTranslations;
        static Guid questionId;
    }
}