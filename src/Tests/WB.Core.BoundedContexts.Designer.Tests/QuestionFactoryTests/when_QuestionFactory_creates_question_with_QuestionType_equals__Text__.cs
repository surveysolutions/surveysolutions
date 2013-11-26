using Machine.Specifications;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionFactoryTests
{
    internal class when_QuestionFactory_creates_question_with_QuestionType_equals__Text__ : QuestionFactoryTestContext
    {
        Establish context = () =>
        {
            textQuestionData = CreateQuestionData(QuestionType: QuestionType.Text);

            factory = CreateQuestionFactory();
        };

        Because of = () =>
            resultQuestion = factory.CreateQuestion(textQuestionData);

        private It should_create_text_question = () =>
            resultQuestion.ShouldBeOfType<TextQuestion>();

        private It should_create_question_with_QuestionType_field_equals__Text__ = () =>
            resultQuestion.QuestionType.ShouldEqual(QuestionType.Text);

        private static QuestionFactory factory;
        private static QuestionData textQuestionData;
        private static IQuestion resultQuestion;
    }
}