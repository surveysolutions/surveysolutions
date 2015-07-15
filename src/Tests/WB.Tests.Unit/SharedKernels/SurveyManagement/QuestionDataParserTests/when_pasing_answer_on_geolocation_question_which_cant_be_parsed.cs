using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_geolocation_question_which_cant_be_parsed : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "unparsed";
            questionDataParser = CreateQuestionDataParser();
            question = new GpsCoordinateQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.GpsCoordinates,
                StataExportCaption = questionVarName
            };
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer,questionVarName+"_aa", question, CreateQuestionnaireDocumentWithOneChapter(question), out parcedValue);

        private It should_result_be_null = () =>
            parsingResult.ShouldEqual(ValueParsingResult.AnswerAsGpsWasNotParsed);
    }
}