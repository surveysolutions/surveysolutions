using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateGpsCoordinatesQuestionHandlerTests
{
    internal class when_updating_gps_coordinates_question_and_all_parameters_is_specified : QuestionnaireTestsContext
    {
        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddQRBarcodeQuestion(
                questionId,
                chapterId,
                title: "old title",
                variableName: "old_variable_name",
                instructions: "old instructions",
                enablementCondition: "old condition",
                responsibleId: responsibleId);
        };

        private Because of = () =>
            questionnaire.UpdateGpsCoordinatesQuestion(
                new UpdateGpsCoordinatesQuestion(
                    questionnaire.Id,
                    questionId: questionId,
                    commonQuestionParameters: new CommonQuestionParameters()
                    {
                        Title = title,
                        VariableName = variableName,
                        VariableLabel = variableLabel,
                        Instructions = instructions,
                        EnablementCondition = enablementCondition,
                        HideIfDisabled = hideIfDisabled

                    }, 
                    validationMessage:null,
                    validationExpression:null,
                    isPreFilled: false,
                    scope: scope,
                    responsibleId: responsibleId,
                    validationConditions: new List<ValidationCondition>()));

        It should_contains_question_with_QuestionId_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .PublicKey.ShouldEqual(questionId);

        It should_contains_question_with_variable_name_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .StataExportCaption.ShouldEqual(variableName);

        It should_contains_question_with_variable_label_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .VariableLabel.ShouldEqual(variableLabel);

        It should_contains_question_with_title_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .QuestionText.ShouldEqual(title);

        It should_contains_question_with_condition_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .ConditionExpression.ShouldEqual(enablementCondition);

        It should_contains_question_with_hideIfDisabled_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .HideIfDisabled.ShouldEqual(hideIfDisabled);

        It should_contains_question_with_instructions_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .Instructions.ShouldEqual(instructions);

        It should_contains_question_with_scope_specified = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .QuestionScope.ShouldEqual(scope);

        private static Questionnaire questionnaire;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string variableLabel = "label";
        private static string instructions = "intructions";
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = "some condition";
        private static bool hideIfDisabled = true;
    }
}