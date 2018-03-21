using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_multy_option_question : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            answer = "2";
            questionDataParser = CreateQuestionDataParser();
            question = new MultyOptionsQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.MultyOption,
                Answers =
                    new List<Answer>()
                    {
                        new Answer() {AnswerValue = "1", AnswerText = "1"},
                        new Answer() {AnswerValue = "2", AnswerText = "2"},
                        new Answer() {AnswerValue = "3", AnswerText = "3"}
                    },
                StataExportCaption = questionVarName
            };
            BecauseOf();
        }

        public void BecauseOf() => parsingResult = questionDataParser.TryParse(answer, questionVarName + "__2", question, out parcedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_result_be_equal_to_2 () => parcedValue.Should().Be((decimal) 2);
    }
}
