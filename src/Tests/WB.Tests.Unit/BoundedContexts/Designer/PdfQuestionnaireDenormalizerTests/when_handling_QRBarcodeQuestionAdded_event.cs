using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_QRBarcodeQuestionAdded_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            pdfQuestionnaireDocument = CreatePdfQuestionnaire(CreateGroup(Guid.Parse(parentGroupId)));

            var documentStorage =
                Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(
                    writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(Create.QRBarcodeQuestionAddedEvent(questionId: questionId, parentGroupId: parentGroupId,
                questionTitle: questionTitle, questionVariable: questionVariable,
                questionConditionExpression: questionEnablementCondition));

        It should_question_not_be_null = () =>
            GetQuestion().ShouldNotBeNull();

        It should_question_type_be_QRBarcode = () =>
            GetQuestion().QuestionType.ShouldEqual(QuestionType.QRBarcode);

        It should_question_title_be_equal_to_specified_title = () =>
            GetQuestion().Title.ShouldEqual(questionTitle);

        It should_question_title_be_equal_to_specified_var_name = () =>
            GetQuestion().VariableName.ShouldEqual(questionVariable);

        It should_question_title_be_equal_to_specified_enablement_condition = () =>
            GetQuestion().ConditionExpression.ShouldEqual(questionEnablementCondition);

        private static PdfQuestionView GetQuestion()
        {
            return pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(Guid.Parse(questionId));
        }

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static string questionId = "11111111111111111111111111111111";
        private static string parentGroupId = "22222222222222222222222222222222";
        private static string questionTitle = "someTitle";
        private static string questionVariable = "var";
        private static string questionEnablementCondition = "some condition";
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}
