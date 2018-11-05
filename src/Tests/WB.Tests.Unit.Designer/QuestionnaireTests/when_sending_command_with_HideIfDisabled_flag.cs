using System;
using System.Collections.Generic;
using FluentAssertions;
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
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();
        }

        private void BecauseOf() 
        {
            questionnaire.UpdateNumericQuestion(
                new UpdateNumericQuestion(
                    questionnaire.Id, questionNumericId, responsibleId,
                    new CommonQuestionParameters() { Title = "title", VariableName = "variableName2" ,VariableLabel = "variableLabel", HideIfDisabled = true},
                    scope: QuestionScope.Interviewer, isInteger: true, countOfDecimalPlaces: null, validationConditions: new List<ValidationCondition>(),
                    isPreFilled:false,useFormatting:false, options: null));

            questionnaire.UpdateDateTimeQuestion(
                new UpdateDateTimeQuestion(questionnaireId: Guid.Parse("22222222222222222222222222222222"), questionId: questionDateTimeId, 
                isPreFilled: false, scope: QuestionScope.Interviewer, responsibleId: responsibleId, validationConditions: new List<ValidationCondition>(), 
                commonQuestionParameters: new CommonQuestionParameters{Title = "title", VariableName = "variableName1",
                VariableLabel = "variableLabel", EnablementCondition = null, HideIfDisabled = true, Instructions = null}, isTimestamp: false, defaultDate: null));

            questionnaire.UpdateGpsCoordinatesQuestion(
                new UpdateGpsCoordinatesQuestion(questionnaire.Id, questionGpsCoordinatesId, responsibleId,
                new CommonQuestionParameters() {Title = "title", VariableName = "variableName3", VariableLabel = "variableLabel3", HideIfDisabled = true}, 
                isPreFilled: false, scope: QuestionScope.Interviewer,   
                validationConditions: new List<ValidationCondition>(),validationExpression:null, validationMessage:null));

            questionnaire.UpdateMultimediaQuestion(
                Create.Command.UpdateMultimediaQuestion(
                questionId: questionMultimediaId, title: "title", variableName: "variableName4", 
                variableLabel: "variableLabel4", enablementCondition: null, hideIfDisabled: true, instructions: null, responsibleId: responsibleId, 
                scope: QuestionScope.Interviewer, properties: Create.QuestionProperties(), isSignature: false));

            questionnaire.UpdateMultiOptionQuestion(questionId: questionMultyOptionId, title: "title", variableName: "variableName5", 
                variableLabel: "variableLabel5", scope: QuestionScope.Interviewer, enablementCondition: null, hideIfDisabled: true, instructions: null, 
                responsibleId: responsibleId, options: new Option[] { new Option("1", "1"), new Option("2", "2"), }, 
                linkedToEntityId: null, areAnswersOrdered: false, maxAllowedAnswers: 2, yesNoView: false,
                validationConditions: new List<ValidationCondition>(), linkedFilterExpression: null, properties: Create.QuestionProperties());

            questionnaire.UpdateQRBarcodeQuestion(
                new UpdateQRBarcodeQuestion(questionnaire.Id,questionQRBarcodeId, 
                responsibleId, new CommonQuestionParameters() {Title = "title",
                    VariableName= "variableName6",
                    VariableLabel= "variableLabel6",
                    HideIfDisabled = true
                }, scope: QuestionScope.Interviewer, validationConditions: new List<ValidationCondition>(), validationExpression:null, validationMessage:null));

            questionnaire.UpdateSingleOptionQuestion(questionId: questionSingleOptionId, title: "title", variableName: "variableName7", 
                variableLabel: "variableLabel7", isPreFilled: false, scope: QuestionScope.Interviewer, enablementCondition: null, 
                hideIfDisabled: true, instructions: null, responsibleId: responsibleId, options: new Option[] {new Option("1", "1"),
                    new Option("2", "2"), }, linkedToEntityId: null, isFilteredCombobox: false,
                cascadeFromQuestionId: null, validationConditions: new List<ValidationCondition>(), linkedFilterExpression: null, 
                properties: Create.QuestionProperties());

            questionnaire.UpdateTextQuestion(
                new UpdateTextQuestion(questionnaire.Id,questionTextId, responsibleId,
                new CommonQuestionParameters() { Title = "title", VariableName= "variableName8", VariableLabel= "variableLabel8", HideIfDisabled = true}, 
                isPreFilled: false, scope: QuestionScope.Interviewer, mask: null,
                validationConditions: new List<ValidationCondition>()));

            questionnaire.UpdateTextListQuestion(
                new UpdateTextListQuestion(questionnaire.Id, questionTextListId,responsibleId,
                    new CommonQuestionParameters() { Title= "title", VariableName= "variableName9",
                    VariableLabel = "variableLabel9", HideIfDisabled = true}, 
                    scope: QuestionScope.Interviewer,  validationConditions: new List<ValidationCondition>(), maxAnswerCount: 30));

            questionnaire.UpdateGroup(hideIfDisabled: true, groupId: groupId, responsibleId: responsibleId, title: "title", variableName: "groupVarName", condition: null, description: null, rosterSizeQuestionId: null, isRoster: true, rosterFixedTitles: new FixedRosterTitleItem[] { new FixedRosterTitleItem("1", "1"), new FixedRosterTitleItem("2", "2"), }, rosterSizeSource: RosterSizeSourceType.FixedTitles, rosterTitleQuestionId: null);
        }


        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified_for_Numeric_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionNumericId).HideIfDisabled.Should().Be(true);

        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified_for_DateTime_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionDateTimeId).HideIfDisabled.Should().Be(true);

        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified_for_Multimedia_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionMultimediaId).HideIfDisabled.Should().Be(true);

        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified_for_GpsCoordinates_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionGpsCoordinatesId).HideIfDisabled.Should().Be(true);

        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified_for_MultyOption_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionMultyOptionId).HideIfDisabled.Should().Be(true);

        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified_for_QRBarcode_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionQRBarcodeId).HideIfDisabled.Should().Be(true);

        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified_for_SingleOption_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionSingleOptionId).HideIfDisabled.Should().Be(true);

        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified_for_Text_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionTextId).HideIfDisabled.Should().Be(true);

        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified_for_TextList_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionTextListId).HideIfDisabled.Should().Be(true);

        [NUnit.Framework.Test] public void should_contains_question_with_hideIfDisabled_specified_for_group_or_roster () =>
            questionnaire.QuestionnaireDocument.Find<IGroup>(groupId).HideIfDisabled.Should().Be(true);


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
