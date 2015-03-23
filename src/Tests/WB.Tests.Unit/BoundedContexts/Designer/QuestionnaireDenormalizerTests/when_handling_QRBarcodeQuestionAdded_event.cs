using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;
namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_QRBarcodeQuestionAdded_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionFactory = new Mock<IQuestionnaireEntityFactory>();

            questionFactory.Setup(x => x.CreateQuestion(it.IsAny<QuestionData>()))
                .Callback((QuestionData qd) => questionData = qd)
                .Returns(CreateQRBarcodeQuestion(
                    questionId: questionId,
                    enablementCondition: condition,
                    isMandatory: isMandatory,
                    instructions: instructions,
                    title: title,
                    variableName: variableName));

            @event = ToPublishedEvent(new QRBarcodeQuestionAdded()
            {
                QuestionId = questionId,
                ParentGroupId = parentGroupId,
                EnablementCondition = condition,
                IsMandatory = isMandatory,
                Instructions = instructions,
                Title = title,
                VariableName = variableName
            });

            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentGroupId)
            });

            var documentStorage = new Mock<IReadSideKeyValueStorage<QuestionnaireDocument>>();
            
            documentStorage
                .Setup(writer => writer.GetById(Moq.It.IsAny<string>()))
                .Returns(questionnaireDocument);
            
            documentStorage
                .Setup(writer => writer.Store(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<string>()))
                .Callback((QuestionnaireDocument document, string id) =>
                {
                    questionnaireView = document;
                });

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage.Object, questionnaireEntityFactory: questionFactory.Object);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_pass_PublicKey_equals_questionId_to_question_factory = () =>
            questionData.PublicKey.ShouldEqual(questionId);

        It should_pass_QuestionType_equals_QRBarcode_to_question_factory = () =>
           questionData.QuestionType.ShouldEqual(QuestionType.QRBarcode);

        It should_pass_QuestionText_equals_questionId_to_question_factory = () =>
           questionData.QuestionText.ShouldEqual(title);

        It should_pass_StataExportCaption_equals_questionId_to_question_factory = () =>
           questionData.StataExportCaption.ShouldEqual(variableName);

        It should_pass_ConditionExpression_equals_questionId_to_question_factory = () =>
           questionData.ConditionExpression.ShouldEqual(condition);

        It should_pass_Mandatory_equals_questionId_to_question_factory = () =>
           questionData.Mandatory.ShouldEqual(isMandatory);

        It should_pass_Instructions_equals_questionId_to_question_factory = () =>
           questionData.Instructions.ShouldEqual(instructions);

        It should_call_question_factory_ones = () =>
            questionFactory.Verify(x => x.CreateQuestion(it.IsAny<QuestionData>()), Times.Once);

        It should__be_added_new_qr_barcode_question_to_questionnaire_document_view = ()=>
            GetQRBarcodeQuestionById().ShouldNotBeNull();

        It should_set_questionId_as_default_value_for__PublicKey__field = () =>
           GetQRBarcodeQuestionById().PublicKey.ShouldEqual(questionId);

        It should_parent_group_exists_in_questionnaire = () =>
           questionnaireView.Find<IGroup>(parentGroupId).ShouldNotBeNull();

        It should_parent_group_contains_qr_barcode_question = () =>
           questionnaireView.Find<IGroup>(parentGroupId).Children[0].PublicKey.ShouldEqual(questionId);

        It should_set_null_as_default_value_for__ValidationExpression__field = () =>
           GetQRBarcodeQuestionById().ValidationExpression.ShouldBeNull();

        It should_set_null_as_default_value_for__ValidationMessage__field = () =>
            GetQRBarcodeQuestionById().ValidationMessage.ShouldBeNull();

        It should_set_Interviewer_as_default_value_for__QuestionScope__field = () =>
            GetQRBarcodeQuestionById().QuestionScope.ShouldEqual(QuestionScope.Interviewer);

        It should_set_false_as_default_value_for__Featured__field = () =>
            GetQRBarcodeQuestionById().Featured.ShouldBeFalse();

        It should_set_QRBarcode_as_default_value_for__QuestionType__field = () =>
            GetQRBarcodeQuestionById().QuestionType.ShouldEqual(QuestionType.QRBarcode);

        It should_set_true_as_value_for__Mandatory__field = () =>
            GetQRBarcodeQuestionById().Mandatory.ShouldEqual(isMandatory);

        It should_set_varibleName_as_value_for__StataExportCaption__field = () =>
            GetQRBarcodeQuestionById().StataExportCaption.ShouldEqual(variableName);

        It should_set_title_as_value_for__QuestionText__field = () =>
            GetQRBarcodeQuestionById().QuestionText.ShouldEqual(title);

        It should_set_instructions_as_value_for__Instructions__field = () =>
            GetQRBarcodeQuestionById().Instructions.ShouldEqual(instructions);

        It should_set_condition_value_for__ConditionExpression__field = () =>
            GetQRBarcodeQuestionById().ConditionExpression.ShouldEqual(condition);

        private static IQRBarcodeQuestion GetQRBarcodeQuestionById()
        {
            return questionnaireView.FirstOrDefault<IQRBarcodeQuestion>(question => question.PublicKey == questionId);
        }

        private static QuestionData questionData;
        private static Mock<IQuestionnaireEntityFactory> questionFactory;
        private static QuestionnaireDocument questionnaireView;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QRBarcodeQuestionAdded> @event;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string variableName = "qr_barcode_question";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
    }
}