using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_parsing_answer_on_numeric_int_roster_size_question_and_answer_is_negative : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "-5";
            questionDataParser = CreateQuestionDataParser();
            question = new NumericQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.Numeric,
                IsInteger = true,
                StataExportCaption = questionVarName
            };
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer,questionVarName, question,
                        CreateQuestionnaireDocumentWithOneChapter(question,
                            new Group("roster group")
                            {
                                PublicKey = Guid.NewGuid(),
                                IsRoster = true,
                                RosterSizeQuestionId = questionId,
                                RosterSizeSource = RosterSizeSourceType.Question
                            }),
                        out parcedValue);

        It should_result_be_equal_to_negative_5 = () =>
            parcedValue.Value.ShouldEqual(-5);

        It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);

        It should_parsing_result_be_equal_to_AnswerIsIncorrectBecauseIsGreaterThanMaxValue = () =>
          parsingResult.ShouldEqual(ValueParsingResult.AnswerIsIncorrectBecauseQuestionIsUsedAsSizeOfRosterGroupAndSpecifiedAnswerIsNegative);
    }
}
