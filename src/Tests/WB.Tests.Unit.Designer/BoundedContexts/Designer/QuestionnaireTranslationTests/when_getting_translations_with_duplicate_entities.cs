using System;
using System.Collections.Generic;
using Machine.Specifications;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTranslationTests
{
    [Subject(typeof(QuestionnaireTranslation))]
    internal class when_getting_translations_with_duplicate_entities : QuestionnaireTranslationTestsContext
    {
        Establish context = () =>
        {
            Guid translationId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            storedTranslations = new List<TranslationDto>
            {
                Create.TranslationDto(type: TranslationType.Title,
                    translation: "title 1",
                    translationId: translationId,
                    questionnaireEntityId: questionId),
                Create.TranslationDto(type: TranslationType.Title,
                    translation: "title 2",
                    translationId: translationId,
                    questionnaireEntityId: questionId),
                Create.TranslationDto(type: TranslationType.Instruction,
                    translation: "instruction 1",
                    translationId: translationId,
                    questionnaireEntityId: questionId),
                Create.TranslationDto(type: TranslationType.Instruction,
                    translation: "instruction 2",
                    translationId: translationId,
                    questionnaireEntityId: questionId),
                Create.TranslationDto(type: TranslationType.OptionTitle,
                    translation: "option1 1",
                    translationId: translationId,
                    questionnaireEntityId: questionId,
                    translationIndex:"1"),
                Create.TranslationDto(type: TranslationType.OptionTitle,
                    translation: "option1 2",
                    translationId: translationId,
                    questionnaireEntityId: questionId,
                    translationIndex:"1"),
                Create.TranslationDto(type: TranslationType.ValidationMessage,
                    translation: "validation message 1",
                    translationId: translationId,
                    questionnaireEntityId: questionId,
                    translationIndex:"1"),
                Create.TranslationDto(type: TranslationType.ValidationMessage,
                    translation: "validation message 2",
                    translationId: translationId,
                    questionnaireEntityId: questionId,
                    translationIndex:"1")
            };
        };

        Because of = () => translation = CreateQuestionnaireTranslation(storedTranslations);

        It should_return_first_translated_title = () => translation.GetTitle(questionId).ShouldEqual("title 1");

        It should_return_first_translated_instruction = () => translation.GetInstruction(questionId).ShouldEqual("instruction 1");

        It should_return_first_translated_option = () => translation.GetAnswerOption(questionId, "1").ShouldEqual("option1 1");

        It should_return_first_translated_validation = () => translation.GetValidationMessage(questionId, 1).ShouldEqual("validation message 1");


        static ITranslation translation;
        static List<TranslationDto> storedTranslations;
        static Guid questionId;
    }
}