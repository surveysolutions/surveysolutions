﻿using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireDenormalizerTests
{
    internal class when_handling_QRBarcodeQuestionCloned_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            @event = ToPublishedEvent(new QRBarcodeQuestionCloned()
            {
                QuestionId = questionId,
                EnablementCondition = condition,
                IsMandatory = isMandatory,
                Instructions = instructions,
                Title = title,
                VariableName = variableName,
                ParentGroupId = parentGroupId,
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex
            });

            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentGroupId,
                    children: new IComposite[]
                    {
                        new NumericQuestion(), 
                        new QRBarcodeQuestion()
                        { 
                            PublicKey = sourceQuestionId, 
                            StataExportCaption = "old_var_name",
                            Mandatory = false,
                            QuestionText = "old title",
                            ConditionExpression = "old condition",
                            Instructions = "old instructions"
                        }
                    })
            });

            var documentStorage = new Mock<IReadSideRepositoryWriter<QuestionnaireDocument>>();
            
            documentStorage
                .Setup(writer => writer.GetById(Moq.It.IsAny<string>()))
                .Returns(questionnaireDocument);
            
            documentStorage
                .Setup(writer => writer.Store(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<string>()))
                .Callback((QuestionnaireDocument document, string id) =>
                {
                    questionnaireView = document;
                });

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage.Object);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should__not_be_null_qr_barcode_question_from_questionnaire__ = ()=>
            GetQRBarcodeQuestionById().ShouldNotBeNull();

        It should_set_questionId_as_default_value_for__PublicKey__field = () =>
           GetQRBarcodeQuestionById().PublicKey.ShouldEqual(questionId);

        It should_parent_group_exists_in_questionnaire = () =>
           questionnaireView.Find<IGroup>(parentGroupId).ShouldNotBeNull();

        It should_parent_group_contains_qr_barcode_question = () =>
           questionnaireView.Find<IGroup>(parentGroupId).Find<IQRBarcodeQuestion>(questionId);

        It should_set_target_index_to_2 = () =>
           questionnaireView.Find<IGroup>(parentGroupId).Children.IndexOf(GetQRBarcodeQuestionById()).ShouldEqual(targetIndex);

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

        private static QuestionnaireDocument questionnaireView;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QRBarcodeQuestionCloned> @event;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid sourceQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string variableName = "qr_barcode_question";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
        private static int targetIndex = 2;
    }
}