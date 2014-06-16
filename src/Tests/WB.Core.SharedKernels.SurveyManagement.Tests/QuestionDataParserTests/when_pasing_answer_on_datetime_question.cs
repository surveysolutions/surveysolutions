using System;
using System.Globalization;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.QuestionDataParserTests
{
    internal class when_pasing_answer_on_datetime_question : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "4/28/2014";
            questionDataParser = CreateQuestionDataParser();
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer, new DateTimeQuestion()
                    {
                        PublicKey = questionId,
                        QuestionType = QuestionType.DateTime,
                        StataExportCaption = questionVarName
                    }, out parcedValue);

        private It should_result_be_equal_to_4_28_2014 = () =>
            parcedValue.Value.ShouldEqual(DateTime.Parse("4/28/2014", CultureInfo.InvariantCulture.DateTimeFormat));

        private It should_result_key_be_equal_to_questionId = () =>
            parcedValue.Key.ShouldEqual(questionId);
    }
}