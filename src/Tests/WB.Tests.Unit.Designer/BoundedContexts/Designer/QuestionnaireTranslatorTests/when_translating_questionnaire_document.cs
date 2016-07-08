using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.Questionnaire.Translator;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTranslatorTests
{
    internal class when_translating_questionnaire_document
    {
        Establish context = () =>
        {
            originalDocument = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Chapter(chapterId: chapter1, title: "chapter 1"),
                Create.Chapter(chapterId: chapter2, title: "chapter 2"),
                Create.StaticText(staticTextId: staticText1, text: "text 1"),
                Create.StaticText(staticTextId: staticText2, text: "text 2"),
                Create.Question(questionId: question1, title: "question 1", instructions: "instruction 1"),
                Create.Question(questionId: question2, title: "question 2", instructions: "instruction 2"),
                Create.SingleOptionQuestion(questionId: singleOptionQuestion, answers: new List<Answer>
                {
                    Create.Answer(stringValue: radioOption1, answer: "radio 1"),
                    Create.Answer(stringValue: radioOption2, answer: "radio 2"),
                }),
                Create.MultipleOptionsQuestion(questionId: multipleOptionsQuestion, answersList: new List<Answer>
                {
                    Create.Answer(stringValue: checkOption1, answer: "check 1"),
                    Create.Answer(stringValue: checkOption2, answer: "check 2"),
                }),
            });

            translation = Mock.Of<IQuestionnaireTranslation>(_
                => _.GetTitle(chapter1) == "глава 1"
                && _.GetTitle(staticText1) == "текст 1"
                && _.GetTitle(question1) == "вопрос 1"
                && _.GetInstruction(question1) == "инструкция 1"
                && _.GetAnswerOption(singleOptionQuestion, radioOption1) == "радио 1"
                && _.GetAnswerOption(multipleOptionsQuestion, checkOption1) == "галочка 1");

            translator = Create.QuestionnaireTranslator();
        };

        Because of = () =>
            translatedDocument = translator.Translate(originalDocument, translation);

        It should_return_translated_document = () =>
            translatedDocument.ShouldNotBeNull();

        It should_translate_chapter_title_which_has_translation = () =>
            translatedDocument.Find<Group>(chapter1).Title.ShouldEqual("глава 1");

        It should_not_translate_chapter_title_which_does_not_have_translation = () =>
            translatedDocument.Find<Group>(chapter2).Title.ShouldEqual("chapter 2");

        It should_translate_static_text_which_has_translation = () =>
            translatedDocument.Find<StaticText>(staticText1).Text.ShouldEqual("текст 1");

        It should_not_translate_static_text_which_does_not_have_translation = () =>
            translatedDocument.Find<StaticText>(staticText2).Text.ShouldEqual("text 2");

        It should_translate_question_text_which_has_translation = () =>
            translatedDocument.Find<IQuestion>(question1).QuestionText.ShouldEqual("вопрос 1");

        It should_not_translate_question_text_which_does_not_have_translation = () =>
            translatedDocument.Find<IQuestion>(question2).QuestionText.ShouldEqual("question 2");

        It should_translate_question_instruction_which_has_translation = () =>
            translatedDocument.Find<IQuestion>(question1).Instructions.ShouldEqual("инструкция 1");

        It should_not_translate_question_instruction_which_does_not_have_translation = () =>
            translatedDocument.Find<IQuestion>(question2).Instructions.ShouldEqual("instruction 2");

        It should_translate_single_option_question_option_which_has_translation = () =>
            translatedDocument.Find<IQuestion>(singleOptionQuestion).Answers.Single(a => a.AnswerValue == radioOption1).AnswerText.ShouldEqual("радио 1");

        It should_not_translate_single_option_question_option_which_does_not_have_translation = () =>
            translatedDocument.Find<IQuestion>(singleOptionQuestion).Answers.Single(a => a.AnswerValue == radioOption2).AnswerText.ShouldEqual("radio 2");

        It should_translate_multiple_options_question_option_which_has_translation = () =>
            translatedDocument.Find<IQuestion>(multipleOptionsQuestion).Answers.Single(a => a.AnswerValue == checkOption1).AnswerText.ShouldEqual("галочка 1");

        It should_not_translate_multiple_options_question_option_which_does_not_have_translation = () =>
            translatedDocument.Find<IQuestion>(multipleOptionsQuestion).Answers.Single(a => a.AnswerValue == checkOption2).AnswerText.ShouldEqual("check 2");

        private static QuestionnaireDocument originalDocument;
        private static QuestionnaireTranslator translator;
        private static IQuestionnaireTranslation translation;
        private static QuestionnaireDocument translatedDocument;
        private static Guid chapter1 = Guid.Parse("1111111111111111AAAAAAAAAAAAAAAA");
        private static Guid chapter2 = Guid.Parse("1111111111111111BBBBBBBBBBBBBBBB");
        private static Guid staticText1 = Guid.Parse("2222222222222222AAAAAAAAAAAAAAAA");
        private static Guid staticText2 = Guid.Parse("2222222222222222BBBBBBBBBBBBBBBB");
        private static Guid question1 = Guid.Parse("3333333333333333AAAAAAAAAAAAAAAA");
        private static Guid question2 = Guid.Parse("3333333333333333BBBBBBBBBBBBBBBB");
        private static Guid singleOptionQuestion = Guid.Parse("44444444444444444444444444444444");
        private static Guid multipleOptionsQuestion = Guid.Parse("55555555555555555555555555555555");
        private static string radioOption1 = "10";
        private static string radioOption2 = "20";
        private static string checkOption1 = "1000";
        private static string checkOption2 = "2000";
    }
}
