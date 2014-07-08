using Machine.Specifications;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireEntityFactoryTests
{
    internal class when_QuestionnaireEntityFactory_creates_question_with_QuestionType_equals__AutoPtopagate__ : QuestionnaireEntityFactoryTestContext
    {
        Establish context = () =>
        {
            autoPropagateQuestionData = CreateQuestionData(questionType: QuestionType.AutoPropagate);

            factory = CreateFactory();
        };

        Because of = () =>
            resultQuestion = factory.CreateQuestion(autoPropagateQuestionData);

        It should_create_numeric_question = () =>
            resultQuestion.ShouldBeOfExactType<NumericQuestion>();

        It should_create_question_with_QuestionType_field_equals__Numeric__ = () =>
           resultQuestion.QuestionType.ShouldEqual(QuestionType.Numeric);

        private static QuestionnaireEntityFactory factory;
        private static QuestionData autoPropagateQuestionData;
        private static IQuestion resultQuestion;
    }
}