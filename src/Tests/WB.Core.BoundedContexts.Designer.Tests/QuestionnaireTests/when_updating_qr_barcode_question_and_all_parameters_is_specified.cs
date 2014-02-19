﻿using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests
{
    internal class when_updating_qr_barcode_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new QRBarcodeQuestionAdded()
            {
                QuestionId = questionId,
                ParentGroupId = chapterId,
                Title = "old title",
                VariableName = "old_variable_name",
                IsMandatory = false,
                Instructions = "old instructions",
                ConditionExpression = "old condition",
                ResponsibleId = responsibleId
            });
            eventContext = new EventContext();
        };

        Because of = () =>            
                questionnaire.UpdateQRBarcodeQuestion(questionId: questionId, title: "title",
                    variableName: "qr_barcode_question", isMandatory: isMandatory, condition: condition, instructions: instructions,
                    responsibleId: responsibleId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QRBarcodeQuestionAdded_event = () =>
            eventContext.ShouldContainEvent<QRBarcodeQuestionUpdated>();

        It should_raise_QRBarcodeQuestionAdded_event_with_QuestionId_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .QuestionId.ShouldEqual(questionId);

        It should_raise_QRBarcodeQuestionAdded_event_with_variable_name_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .VariableName.ShouldEqual(variableName);

        It should_raise_QRBarcodeQuestionAdded_event_with_title_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .Title.ShouldEqual(title);

        It should_raise_QRBarcodeQuestionAdded_event_with_condition_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .ConditionExpression.ShouldEqual(condition);

        It should_raise_QRBarcodeQuestionAdded_event_with_ismandatory_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .IsMandatory.ShouldEqual(isMandatory);

        It should_raise_QRBarcodeQuestionAdded_event_with_instructions_specified = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionUpdated>()
                .Instructions.ShouldEqual(instructions);

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static bool isMandatory = true;
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";
    }
}