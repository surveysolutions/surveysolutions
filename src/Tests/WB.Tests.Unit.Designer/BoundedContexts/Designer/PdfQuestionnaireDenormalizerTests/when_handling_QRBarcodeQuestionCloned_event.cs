using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_QRBarcodeQuestionCloned_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            pdfQuestionnaireDocument = CreatePdfQuestionnaire(CreateGroup(Guid.Parse(parentGroupId)));

            validationExpression = "expression 1";
            validationMessage = "message 1";

            var documentStorage =
                Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(
                    writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        private Because of = () =>
            denormalizer.Handle(Create.QRBarcodeQuestionClonedEvent(questionId: questionId, parentGroupId: parentGroupId,
                questionTitle: questionTitle, questionVariable: questionVariable,
                questionConditionExpression: questionEnablementCondition,
                validationConditions: new List<ValidationCondition>
                {
                    new ValidationCondition
                    {
                        Expression = validationExpression,
                        Message = validationMessage
                    }
                }));

        It should_question_not_be_null = () =>
            GetQuestion().ShouldNotBeNull();

        It should_question_type_be_QRBarcode = () =>
            GetQuestion().QuestionType.ShouldEqual(PdfQuestionType.QRBarcode);

        It should_question_title_be_equal_to_specified_title = () =>
            GetQuestion().Title.ShouldEqual(questionTitle);

        It should_question_title_be_equal_to_specified_var_name = () =>
            GetQuestion().VariableName.ShouldEqual(questionVariable);

        It should_question_title_be_equal_to_specified_enablement_condition = () =>
            GetQuestion().ConditionExpression.ShouldEqual(questionEnablementCondition);

        It should_set_validation_condtions_from_event = () => GetQuestion().ValidationConditions.Count.ShouldEqual(1);

        It should_update_validation_condition_properties = () =>
        {
            GetQuestion().ValidationConditions.First().Expression.ShouldEqual(validationExpression);
            GetQuestion().ValidationConditions.First().Message.ShouldEqual(validationMessage);
        };

        static PdfQuestionView GetQuestion()
        {
            return pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(Guid.Parse(questionId));
        }

        static PdfQuestionnaireDenormalizer denormalizer;
        static string questionId = "11111111111111111111111111111111";
        static string parentGroupId = "22222222222222222222222222222222";
        static string questionTitle = "someTitle";
        static string questionVariable = "var";
        static string questionEnablementCondition = "some condition";
        static PdfQuestionnaireView pdfQuestionnaireDocument;
        static string validationExpression;
        static string validationMessage;
    }
}
