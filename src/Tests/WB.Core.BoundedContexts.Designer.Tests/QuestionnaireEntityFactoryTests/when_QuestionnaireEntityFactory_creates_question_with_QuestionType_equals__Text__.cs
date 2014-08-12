using Machine.Specifications;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireEntityFactoryTests
{
    internal class when_QuestionnaireEntityFactory_creates_question_with_QuestionType_equals__Text__ : QuestionnaireEntityFactoryTestContext
    {
        Establish context = () =>
        {
            textQuestionData = CreateQuestionData(questionType: QuestionType.Text);

            factory = CreateFactory();
        };

        Because of = () =>
            resultQuestion = factory.CreateQuestion(textQuestionData);

        It should_create_text_question = () =>
            resultQuestion.ShouldBeOfExactType<TextQuestion>();

        It should_create_question_with_QuestionType_field_equals__Text__ = () =>
            resultQuestion.QuestionType.ShouldEqual(QuestionType.Text);

        private static QuestionnaireEntityFactory factory;
        private static QuestionData textQuestionData;
        private static IQuestion resultQuestion;
    }
}