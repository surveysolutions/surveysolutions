using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_geolocation_question_which_cant_be_parsed : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            answer = "unparsed";
            questionDataParser = CreateQuestionDataParser();
            question = new GpsCoordinateQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.GpsCoordinates,
                StataExportCaption = questionVarName
            };
            BecauseOf();
        }

        private void BecauseOf() =>
                parsingResult =
                    questionDataParser.TryParse(answer,questionVarName+"_aa", question, out parcedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_result_be_null () =>
            parsingResult.Should().Be(ValueParsingResult.AnswerAsGpsWasNotParsed);
    }
}
