using System;
using Machine.Specifications;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireEntityFactoryTests
{
    internal class when_QuestionnaireEntityFactory_creates_question_with_QuestionType_equals__QRBarcode__ : QuestionnaireEntityFactoryTestContext
    {
        Establish context = () =>
        {
            textQuestionData = CreateQuestionData(
                questionType: QuestionType.QRBarcode,
                questionId: questionId,
                variable: variableName,
                condition: condition,
                title: title,
                instructions: instructions);

            factory = CreateFactory();
        };

        Because of = () =>
            resultQuestion = factory.CreateQuestion(textQuestionData);

        It should_create_text_question = () =>
            resultQuestion.ShouldBeOfExactType<QRBarcodeQuestion>();

        It should_create_question_with_QuestionType_field_equals__Text__ = () =>
            resultQuestion.QuestionType.ShouldEqual(QuestionType.QRBarcode);

        It should_create_question_with_PublicKey_field_equals_questionId_ = () =>
           resultQuestion.PublicKey.ShouldEqual(questionId);

        It should_create_question_with_StataExportCaption_field_equals_variableName_ = () =>
           resultQuestion.StataExportCaption.ShouldEqual(variableName);

        It should_create_question_with_StataExportCaption_field_equals_condition_ = () =>
            resultQuestion.ConditionExpression.ShouldEqual(condition);

        It should_create_question_withQuestionText_field_equals_title = () =>
            resultQuestion.QuestionText.ShouldEqual(title);

        It should_create_question_with_Instructions_field_equals_instructions = () =>
            resultQuestion.Instructions.ShouldEqual(instructions);

        private static QuestionnaireEntityFactory factory;
        private static QuestionData textQuestionData;
        private static IQuestion resultQuestion;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
    }
}