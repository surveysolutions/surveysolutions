using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests
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

            eventContext = new EventContext();
        };

        private Because of = () =>
        {
            questionnaire.UpdateNumericQuestion(hideIfDisabled: true, questionId: questionNumericId, isInteger: true, countOfDecimalPlaces: null, title: "title", variableName: "variableName2", variableLabel: "variableLabel", isPreFilled: false, scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>());
            questionnaire.UpdateDateTimeQuestion(hideIfDisabled: true, questionId: questionDateTimeId, title: "title", variableName: "variableName1", variableLabel: "variableLabel", isPreFilled: false, scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>());
            questionnaire.UpdateGpsCoordinatesQuestion(hideIfDisabled: true, questionId: questionGpsCoordinatesId, title: "title", variableName: "variableName3", variableLabel: "variableLabel3", isPreFilled: false, scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>());
            questionnaire.UpdateMultimediaQuestion(hideIfDisabled: true, questionId: questionMultimediaId, title: "title", variableName: "variableName4", variableLabel: "variableLabel4", scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId);
            questionnaire.UpdateMultiOptionQuestion(hideIfDisabled: true, questionId: questionMultyOptionId, title: "title", variableName: "variableName5", variableLabel: "variableLabel5", scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>(), options: new Option[] { new Option(Guid.NewGuid(), "1", "1"), new Option(Guid.NewGuid(), "2", "2"), }, linkedToEntityId: null, areAnswersOrdered: false, maxAllowedAnswers:2, yesNoView:false,
                linkedFilterExpression: null);
            questionnaire.UpdateQRBarcodeQuestion(hideIfDisabled: true, questionId: questionQRBarcodeId, title: "title", variableName: "variableName6", variableLabel: "variableLabel6", scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>());
            questionnaire.UpdateSingleOptionQuestion(hideIfDisabled: true, questionId: questionSingleOptionId, title: "title", variableName: "variableName7", variableLabel: "variableLabel7", isPreFilled: false, scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>(), options: new Option[] {new Option(Guid.NewGuid(), "1", "1"), new Option(Guid.NewGuid(), "2", "2"), }, isFilteredCombobox: false, linkedToEntityId: null, cascadeFromQuestionId: null,
                linkedFilterExpression: null);
            questionnaire.UpdateTextQuestion(hideIfDisabled: true, questionId: questionTextId, title: "title", variableName: "variableName8", variableLabel: "variableLabel8", isPreFilled: false, scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId, mask:null, validationCoditions: new List<ValidationCondition>());
            questionnaire.UpdateTextListQuestion(hideIfDisabled: true, questionId: questionTextListId, title: "title", variableName: "variableName9", variableLabel: "variableLabel9", scope: QuestionScope.Interviewer, enablementCondition: null, instructions: null, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>(), maxAnswerCount: 30);

            questionnaire.UpdateGroup(hideIfDisabled: true, groupId: groupId, responsibleId: responsibleId, title: "title", variableName: "groupVarName", condition: null, description: null, rosterSizeQuestionId: null, isRoster: true, rosterFixedTitles: new FixedRosterTitleItem[] { new FixedRosterTitleItem("1", "1"), new FixedRosterTitleItem("2", "2"), }, rosterSizeSource: RosterSizeSourceType.FixedTitles, rosterTitleQuestionId: null);
        };


        It should_raise_QuestionChanged_event_with_hideIfDisabled_specified_for_Numeric_question = () =>
            eventContext.GetEvents<NumericQuestionChanged>().Single(qc => qc.PublicKey == questionNumericId).HideIfDisabled.ShouldEqual(true);

        It should_raise_QuestionChanged_event_with_hideIfDisabled_specified_for_DateTime_question = () =>
            eventContext.GetEvents<QuestionChanged>().Single(qc => qc.PublicKey == questionDateTimeId).HideIfDisabled.ShouldEqual(true);

        It should_raise_QuestionChanged_event_with_hideIfDisabled_specified_for_Multimedia_question = () =>
            eventContext.GetEvents<MultimediaQuestionUpdated>().Single(qc => qc.QuestionId == questionMultimediaId).HideIfDisabled.ShouldEqual(true);

        It should_raise_QuestionChanged_event_with_hideIfDisabled_specified_for_GpsCoordinates_question = () =>
            eventContext.GetEvents<QuestionChanged>().Single(qc => qc.PublicKey == questionGpsCoordinatesId).HideIfDisabled.ShouldEqual(true);

        It should_raise_QuestionChanged_event_with_hideIfDisabled_specified_for_MultyOption_question = () =>
            eventContext.GetEvents<QuestionChanged>().Single(qc => qc.PublicKey == questionMultyOptionId).HideIfDisabled.ShouldEqual(true);

        It should_raise_QuestionChanged_event_with_hideIfDisabled_specified_for_QRBarcode_question = () =>
            eventContext.GetEvents<QRBarcodeQuestionUpdated>().Single(qc => qc.QuestionId == questionQRBarcodeId).HideIfDisabled.ShouldEqual(true);

        It should_raise_QuestionChanged_event_with_hideIfDisabled_specified_for_SingleOption_question = () =>
            eventContext.GetEvents<QuestionChanged>().Single(qc => qc.PublicKey == questionSingleOptionId).HideIfDisabled.ShouldEqual(true);

        It should_raise_QuestionChanged_event_with_hideIfDisabled_specified_for_Text_question = () =>
            eventContext.GetEvents<QuestionChanged>().Single(qc => qc.PublicKey == questionTextId).HideIfDisabled.ShouldEqual(true);

        It should_raise_QuestionChanged_event_with_hideIfDisabled_specified_for_TextList_question = () =>
            eventContext.GetEvents<TextListQuestionChanged>().Single(qc => qc.PublicKey == questionTextListId).HideIfDisabled.ShouldEqual(true);

        It should_raise_QuestionChanged_event_with_hideIfDisabled_specified_for_group_or_roster = () =>
            eventContext.GetEvents<GroupUpdated>().Single(qc => qc.GroupPublicKey == groupId).HideIfDisabled.ShouldEqual(true);


        private static EventContext eventContext;
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