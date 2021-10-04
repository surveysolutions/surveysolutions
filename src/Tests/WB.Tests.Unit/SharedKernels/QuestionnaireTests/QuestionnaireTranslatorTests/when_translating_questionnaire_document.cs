using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.QuestionnaireTests.QuestionnaireTranslatorTests
{
    internal class when_translating_questionnaire_document
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            originalDocument = Create.Entity.QuestionnaireDocument(
                title: "Non translated",
                id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.Group(groupId: chapter1, title: "chapter 1"),
                    Create.Entity.Group(groupId: chapter2, title: "chapter 2"),
                    Create.Entity.StaticText(publicKey: staticText1, text: "text 1"),
                    Create.Entity.StaticText(publicKey: staticText2, text: "text 2"),
                    Create.Entity.TextQuestion(questionId: question1, text: "question 1", instruction: "instruction 1"),
                    Create.Entity.TextQuestion(questionId: question2, text: "question 2", instruction: "instruction 2"),
                    Create.Entity.SingleOptionQuestion(questionId: singleOptionQuestion, answers: new List<Answer>
                    {
                        Create.Entity.Answer(value: radioOption1, answer: "radio 1"),
                        Create.Entity.Answer(value: radioOption2, answer: "radio 2"),
                    }),
                    Create.Entity.MultipleOptionsQuestion(questionId: multipleOptionsQuestion, textAnswers: new[]
                    {
                        Create.Entity.Answer(value: checkOption1, answer: "check 1"),
                        Create.Entity.Answer(value: checkOption2, answer: "check 2"),
                    }),
                    Create.Entity.TextQuestion(questionId: questionWithValidations, validationConditions: new[]
                    {
                        Create.Entity.ValidationCondition(message: "question validation 1"),
                        Create.Entity.ValidationCondition(message: "question validation 2"),
                    }),
                    Create.Entity.StaticText(publicKey: staticTextWithValidations, validationConditions: new List<ValidationCondition>
                    {
                        Create.Entity.ValidationCondition(message: "text validation 1"),
                        Create.Entity.ValidationCondition(message: "text validation 2"),
                    }),
                    Create.Entity.FixedRoster(rosterId: fixedRoster, fixedTitles: new[]
                    {
                        Create.Entity.FixedTitle(rosterTitleValue1, "roster title 1"),
                        Create.Entity.FixedTitle(rosterTitleValue2, "roster title 2"),
                    }),
            });

            translation = Mock.Of<ITranslation>(_
                => _.GetTitle(chapter1) == "глава 1"
                && _.GetTitle(staticText1) == "текст 1"
                && _.GetTitle(question1) == "вопрос 1"
                && _.GetInstruction(question1) == "инструкция 1"
                && _.GetAnswerOption(singleOptionQuestion, radioOption1.ToString(), null) == "радио 1"
                && _.GetAnswerOption(multipleOptionsQuestion, checkOption1.ToString(), null) == "галочка 1"
                && _.GetValidationMessage(questionWithValidations, 1) == "валидация вопроса 1"
                && _.GetValidationMessage(staticTextWithValidations, 1) == "валидация текста 1"
                && _.GetFixedRosterTitle(fixedRoster, rosterTitleValue1) == "титул ростера 1"
                && _.GetTitle(questionnaireId) == "заголовок опросника");

            translator = Create.Service.QuestionnaireTranslator();
            BecauseOf();
        }

        private void BecauseOf() =>
            translatedDocument = translator.Translate(originalDocument, translation);

        [NUnit.Framework.Test] public void should_return_translated_document () =>
            translatedDocument.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_translate_chapter_title_which_has_translation () =>
            translatedDocument.Find<Group>(chapter1).Title.Should().Be("глава 1");

        [NUnit.Framework.Test] public void should_not_translate_chapter_title_which_does_not_have_translation () =>
            translatedDocument.Find<Group>(chapter2).Title.Should().Be("chapter 2");

        [NUnit.Framework.Test] public void should_translate_static_text_which_has_translation () =>
            translatedDocument.Find<StaticText>(staticText1).Text.Should().Be("текст 1");

        [NUnit.Framework.Test] public void should_not_translate_static_text_which_does_not_have_translation () =>
            translatedDocument.Find<StaticText>(staticText2).Text.Should().Be("text 2");

        [NUnit.Framework.Test] public void should_translate_question_text_which_has_translation () =>
            translatedDocument.Find<IQuestion>(question1).QuestionText.Should().Be("вопрос 1");

        [NUnit.Framework.Test] public void should_not_translate_question_text_which_does_not_have_translation () =>
            translatedDocument.Find<IQuestion>(question2).QuestionText.Should().Be("question 2");

        [NUnit.Framework.Test] public void should_translate_question_instruction_which_has_translation () =>
            translatedDocument.Find<IQuestion>(question1).Instructions.Should().Be("инструкция 1");

        [NUnit.Framework.Test] public void should_not_translate_question_instruction_which_does_not_have_translation () =>
            translatedDocument.Find<IQuestion>(question2).Instructions.Should().Be("instruction 2");

        [NUnit.Framework.Test] public void should_translate_single_option_question_option_which_has_translation () =>
            translatedDocument.Find<IQuestion>(singleOptionQuestion).Answers.Single(a => a.AnswerCode == radioOption1).AnswerText.Should().Be("радио 1");

        [NUnit.Framework.Test] public void should_not_translate_single_option_question_option_which_does_not_have_translation () =>
            translatedDocument.Find<IQuestion>(singleOptionQuestion).Answers.Single(a =>  a.AnswerCode== radioOption2).AnswerText.Should().Be("radio 2");

        [NUnit.Framework.Test] public void should_translate_multiple_options_question_option_which_has_translation () =>
            translatedDocument.Find<IQuestion>(multipleOptionsQuestion).Answers.Single(a =>  a.AnswerCode == checkOption1).AnswerText.Should().Be("галочка 1");

        [NUnit.Framework.Test] public void should_not_translate_multiple_options_question_option_which_does_not_have_translation () =>
            translatedDocument.Find<IQuestion>(multipleOptionsQuestion).Answers.Single(a =>  a.AnswerCode == checkOption2).AnswerText.Should().Be("check 2");

        [NUnit.Framework.Test] public void should_translate_question_validation_message_which_has_translation () =>
            translatedDocument.Find<IQuestion>(questionWithValidations).ValidationConditions[0].Message.Should().Be("валидация вопроса 1");

        [NUnit.Framework.Test] public void should_not_translate_question_validation_message_which_does_not_have_translation () =>
            translatedDocument.Find<IQuestion>(questionWithValidations).ValidationConditions[1].Message.Should().Be("question validation 2");

        [NUnit.Framework.Test] public void should_translate_static_text_validation_message_which_has_translation () =>
            translatedDocument.Find<IStaticText>(staticTextWithValidations).ValidationConditions[0].Message.Should().Be("валидация текста 1");

        [NUnit.Framework.Test] public void should_not_translate_static_text_validation_message_which_does_not_have_translation () =>
            translatedDocument.Find<IStaticText>(staticTextWithValidations).ValidationConditions[1].Message.Should().Be("text validation 2");

        [NUnit.Framework.Test] public void should_translate_fixed_roster_title_which_has_translation () =>
            translatedDocument.Find<IGroup>(fixedRoster).FixedRosterTitles.Single(t => t.Value == rosterTitleValue1).Title.Should().Be("титул ростера 1");

        [NUnit.Framework.Test] public void should_not_translate_fixed_roster_title_which_does_not_have_translation () =>
            translatedDocument.Find<IGroup>(fixedRoster).FixedRosterTitles.Single(t => t.Value == rosterTitleValue2).Title.Should().Be("roster title 2");

        [Test]
        public void should_translate_questionnaire_title() => translatedDocument.Title.Should().Be("заголовок опросника");

        private QuestionnaireDocument originalDocument;
        private ITranslation translation;
        private QuestionnaireDocument translatedDocument;
        private Guid chapter1 = Guid.Parse("1111111111111111AAAAAAAAAAAAAAAA");
        private Guid chapter2 = Guid.Parse("1111111111111111BBBBBBBBBBBBBBBB");
        private Guid staticText1 = Guid.Parse("2222222222222222AAAAAAAAAAAAAAAA");
        private Guid staticText2 = Guid.Parse("2222222222222222BBBBBBBBBBBBBBBB");
        private Guid question1 = Guid.Parse("3333333333333333AAAAAAAAAAAAAAAA");
        private Guid question2 = Guid.Parse("3333333333333333BBBBBBBBBBBBBBBB");
        private Guid singleOptionQuestion = Guid.Parse("44444444444444444444444444444444");
        private Guid multipleOptionsQuestion = Guid.Parse("55555555555555555555555555555555");
        private Guid questionWithValidations = Guid.Parse("66666666666666666666666666666666");
        private Guid staticTextWithValidations = Guid.Parse("77777777777777777777777777777777");
        private Guid fixedRoster = Guid.Parse("88888888888888888888888888888888");
        private Guid questionnaireId = Id.g1;
        private int radioOption1 = 10;
        private int radioOption2 = 20;
        private int checkOption1 = 1000;
        private int checkOption2 = 2000;
        private int rosterTitleValue1 = 100000;
        private int rosterTitleValue2 = 200000;
        private QuestionnaireTranslator translator;
    }
}
