using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_single_option_question : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            answer = "2";
            questionDataParser = CreateQuestionDataParser();
            question = new SingleQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.SingleOption,
                Answers =
                    new List<Answer>()
                    {
                        new Answer() {AnswerValue = "1", AnswerText = "1"},
                        new Answer() {AnswerValue = "2", AnswerText = "2"}
                    },
                StataExportCaption = questionVarName
            };
            BecauseOf();
        }

        private  void BecauseOf() =>
                parsingResult =
                    questionDataParser.TryParse(answer, questionVarName, question, out parcedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_result_be_equal_to_2 () =>
            parcedValue.Should().Be((decimal) 2);
    }
}
