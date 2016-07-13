using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTranslationTests
{
    [Subject(typeof(QuestionnaireTranslation))]
    internal class when_getting_translations : QuestionnaireTranslationTestsContext
    {
        Establish context = () =>
        {
            string culture = "en-US";
            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            storedTranslations = new List<TranslationDto>
            {
                Create.TranslationDto(type: TranslationType.Title,
                    translation: "title",
                    culture: culture,
                    questionnaireEntityId: questionId),
                Create.TranslationDto(type: TranslationType.Instruction,
                    translation: "instruction",
                    culture: culture,
                    questionnaireEntityId: questionId),
                Create.TranslationDto(type: TranslationType.OptionTitle,
                    translation: "option1",
                    culture: culture,
                    questionnaireEntityId: questionId,
                    translationIndex:"1"),
                Create.TranslationDto(type: TranslationType.ValidationMessage,
                    translation: "validation message",
                    culture: culture,
                    questionnaireEntityId: questionId,
                    translationIndex:"1")
            };
        };

        Because of = () => translation = CreateQuestionnaireTranslation(storedTranslations);

        It should_return_translated_title = () => translation.GetTitle(questionId).ShouldEqual("title");

        It should_return_translated_instruction = () => translation.GetInstruction(questionId).ShouldEqual("instruction");

        It should_return_translated_option = () => translation.GetAnswerOption(questionId, "1").ShouldEqual("option1");

        It should_return_translated_validation = () => translation.GetValidationMessage(questionId, 1).ShouldEqual("validation message");

        It should_return_null_for_missing_title = () => translation.GetTitle(Guid.NewGuid()).ShouldBeNull();

        It should_return_null_for_missing_instruction = () => translation.GetInstruction(Guid.NewGuid()).ShouldBeNull();

        It should_return_null_for_missing_translated_option = () => translation.GetAnswerOption(Guid.NewGuid(), "1").ShouldBeNull();

        It should_return_null_for_missing_translated_validation = () => translation.GetValidationMessage(Guid.NewGuid(), 1).ShouldBeNull();

        static ITranslation translation;
        static List<TranslationDto> storedTranslations;
        static Guid questionId;
    }
}