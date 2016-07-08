using System;
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
                Create.Question(questionId: question1, title: "question 1"),
                Create.Question(questionId: question2, title: "question 2"),
            });

            translation = Mock.Of<IQuestionnaireTranslation>(_
                => _.GetTitle(chapter1) == "глава 1"
                && _.GetTitle(staticText1) == "текст 1"
                && _.GetTitle(question1) == "вопрос 1");

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

        private static QuestionnaireDocument originalDocument;
        private static QuestionnaireTranslator translator;
        private static IQuestionnaireTranslation translation;
        private static QuestionnaireDocument translatedDocument;
        private static Guid chapter1 = Guid.Parse("AAAAAAAAAAAAAAAA1111111111111111");
        private static Guid chapter2 = Guid.Parse("AAAAAAAAAAAAAAAA2222222222222222");
        private static Guid staticText1 = Guid.Parse("BBBBBBBBBBBBBBBB1111111111111111");
        private static Guid staticText2 = Guid.Parse("BBBBBBBBBBBBBBBB2222222222222222");
        private static Guid question1 = Guid.Parse("CCCCCCCCCCCCCCCC1111111111111111");
        private static Guid question2 = Guid.Parse("CCCCCCCCCCCCCCCC2222222222222222");
    }
}
