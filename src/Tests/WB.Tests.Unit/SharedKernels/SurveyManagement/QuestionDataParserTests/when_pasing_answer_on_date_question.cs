using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_date_question : QuestionDataParserTestContext
    {
        Establish context = () =>
        {
            answer = "2016-01-14";
            questionDataParser = CreateQuestionDataParser();
            question = new DateTimeQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = questionVarName
            };
        };

        Because of = () => parsingResult = questionDataParser.TryParse(answer, questionVarName, question, out parcedValue);

        It should_result_be_parsed_successfully = () =>
            parsingResult.ShouldEqual(ValueParsingResult.OK);
    }
}