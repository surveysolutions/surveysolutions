using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_sending_command_with_HideIfDisabled_flag : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            responsibleId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaireWithOneGroup(responsibleId, questionnaireId: Guid.NewGuid(), groupId: groupId, isRoster:false);
            AddQuestion(questionnaire, questionDateTimeId, groupId, responsibleId, QuestionType.DateTime, "datetime_question");
            AddQuestion(questionnaire, questionNumericId, groupId, responsibleId, QuestionType.Numeric, "numeric_question");
            AddQuestion(questionnaire, questionGpsCoordinatesId, groupId, responsibleId, QuestionType.GpsCoordinates, "GpsCoordinates_question");
            AddQuestion(questionnaire, questionMultimediaId, groupId, responsibleId, QuestionType.Multimedia, "Multimedia_question");
            AddQuestion(questionnaire, questionMultyOptionId, groupId, responsibleId, QuestionType.MultyOption, "MultyOption_question");
            AddQuestion(questionnaire, questionQRBarcodeId, groupId, responsibleId, QuestionType.QRBarcode, "QRBarcode_question");
            AddQuestion(questionnaire, questionSingleOptionId, groupId, responsibleId, QuestionType.SingleOption, "SingleOption_question");
            AddQuestion(questionnaire, questionTextId, groupId, responsibleId, QuestionType.Text, "Text_question");
            AddQuestion(questionnaire, questionTextListId, groupId, responsibleId, QuestionType.TextList, "TextList_question");
        };

        private Because of = () =>
        {
            questionnaire.UpdateNumericQuestion(questionId: questionNumericId, title: "title", variableName: "variableName2", variableLabel: "variableLabel", isPreFilled: false, scope: QuestionScope.Interviewer, enablementCondition: null, hideIfDisabled: true, instructions: null, properties: Create.QuestionProperties(), responsibleId: responsibleId, isInteger: true, countOfDecimalPlaces: null, validationConditions: new List<ValidationCondition>());
            questionnaire.UpdateDateTimeQuestion(new UpdateDateTimeQuestion(questionnaireId: Guid.Parse("22222222222222222222222222222222"), questionId: questionDateTimeId, isPreFilled: false, scope: QuestionScope.Interviewer, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>(), commonQuestionParameters: new CommonQuestionParameters{Title = "title", VariableName = "variableName1", VariableLabel = "variableLabel", EnablementCondition = null, HideIfDisabled = true, Instructions = null}, isTimestamp: false));
            questionnaire.UpdateGpsCoordinatesQuestion(questionId: questionGpsCoordinatesId, title: "title", variableName: "variableName3", variableLabel: "variableLabel3", isPreFilled: false, scope: QuestionScope.Interviewer, enablementCondition: null, hideIfDisabled: true, instructions: null, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>(), properties: Create.QuestionProperties());
            questionnaire.UpdateMultimediaQuestion(questionId: questionMultimediaId, title: "title", variableName: "variableName4", variableLabel: "variableLabel4", enablementCondition: null, hideIfDisabled: true, instructions: null, responsibleId: responsibleId, scope: QuestionScope.Interviewer, properties: Create.QuestionProperties());
            questionnaire.UpdateMultiOptionQuestion(questionId: questionMultyOptionId, title: "title", variableName: "variableName5", variableLabel: "variableLabel5", scope: QuestionScope.Interviewer, enablementCondition: null, hideIfDisabled: true, instructions: null, responsibleId: responsibleId, options: new Option[] { new Option(Guid.NewGuid(), "1", "1"), new Option(Guid.NewGuid(), "2", "2"), }, linkedToEntityId: null, areAnswersOrdered: false, maxAllowedAnswers: 2, yesNoView: false,
                validationConditions: new List<ValidationCondition>(), linkedFilterExpression: null, properties: Create.QuestionProperties());
            questionnaire.UpdateQRBarcodeQuestion(hideIfDisabled: true, questionId: questionQRBarcodeId, title: "title", variableName: "variableName6", variableLabel: "variableLabel6", scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>(), properties: Create.QuestionProperties());
            questionnaire.UpdateSingleOptionQuestion(questionId: questionSingleOptionId, title: "title", variableName: "variableName7", variableLabel: "variableLabel7", isPreFilled: false, scope: QuestionScope.Interviewer, enablementCondition: null, hideIfDisabled: true, instructions: null, responsibleId: responsibleId, options: new Option[] {new Option(Guid.NewGuid(), "1", "1"), new Option(Guid.NewGuid(), "2", "2"), }, linkedToEntityId: null, isFilteredCombobox: false,
                cascadeFromQuestionId: null, validationConditions: new List<ValidationCondition>(), linkedFilterExpression: null, properties: Create.QuestionProperties());
            questionnaire.UpdateTextQuestion(questionId: questionTextId, title: "title", variableName: "variableName8", variableLabel: "variableLabel8", isPreFilled: false, scope: QuestionScope.Interviewer, enablementCondition: null, hideIfDisabled: true, instructions: null, mask: null, responsibleId: responsibleId, validationCoditions: new List<ValidationCondition>(), properties: Create.QuestionProperties());
            questionnaire.UpdateTextListQuestion(hideIfDisabled: true, questionId: questionTextListId, title: "title", variableName: "variableName9", variableLabel: "variableLabel9", scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>(), maxAnswerCount: 30, properties: Create.QuestionProperties());

            questionnaire.UpdateGroup(hideIfDisabled: true, groupId: groupId, responsibleId: responsibleId, title: "title", variableName: "groupVarName", condition: null, description: null, rosterSizeQuestionId: null, isRoster: true, rosterFixedTitles: new FixedRosterTitleItem[] { new FixedRosterTitleItem("1", "1"), new FixedRosterTitleItem("2", "2"), }, rosterSizeSource: RosterSizeSourceType.FixedTitles, rosterTitleQuestionId: null);
        };


        It should_contains_question_with_hideIfDisabled_specified_for_Numeric_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionNumericId).HideIfDisabled.ShouldEqual(true);

        It should_contains_question_with_hideIfDisabled_specified_for_DateTime_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionDateTimeId).HideIfDisabled.ShouldEqual(true);

        It should_contains_question_with_hideIfDisabled_specified_for_Multimedia_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionMultimediaId).HideIfDisabled.ShouldEqual(true);

        It should_contains_question_with_hideIfDisabled_specified_for_GpsCoordinates_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionGpsCoordinatesId).HideIfDisabled.ShouldEqual(true);

        It should_contains_question_with_hideIfDisabled_specified_for_MultyOption_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionMultyOptionId).HideIfDisabled.ShouldEqual(true);

        It should_contains_question_with_hideIfDisabled_specified_for_QRBarcode_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionQRBarcodeId).HideIfDisabled.ShouldEqual(true);

        It should_contains_question_with_hideIfDisabled_specified_for_SingleOption_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionSingleOptionId).HideIfDisabled.ShouldEqual(true);

        It should_contains_question_with_hideIfDisabled_specified_for_Text_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionTextId).HideIfDisabled.ShouldEqual(true);

        It should_contains_question_with_hideIfDisabled_specified_for_TextList_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionTextListId).HideIfDisabled.ShouldEqual(true);

        It should_contains_question_with_hideIfDisabled_specified_for_group_or_roster = () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).HideIfDisabled.ShouldEqual(true);


        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;

        private static Guid questionDateTimeId = Guid.NewGuid();
        private static Guid questionNumericId = Guid.NewGuid();
        private static Guid questionMultimediaId = Guid.NewGuid();
        private static Guid questionGpsCoordinatesId = Guid.NewGuid();
        private static Guid questionMultyOptionId = Guid.NewGuid();
        private static Guid questionQRBarcodeId = Guid.NewGuid();
        private static Guid questionSingleOptionId = Guid.NewGuid();
        private static Guid questionTextId = Guid.NewGuid();
        private static Guid questionTextListId = Guid.NewGuid();
    }
}