using System;
using System.Globalization;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_datetime_question_in_iso_format : QuestionDataParserTestContext
    {
        private Establish context = () =>
        {
            answer = "2016-02-02T17:04:41";
            questionDataParser = CreateQuestionDataParser();
            question = new DateTimeQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = questionVarName
            };
        };

        private Because of =
            () =>
                parsingResult =
                    questionDataParser.TryParse(answer,questionVarName, question, out parcedValue);

        private It should_result_be_equal_to_2016_02_02T17_04_41 = () =>
            parcedValue.ShouldEqual(DateTime.Parse("2016-02-02T17:04:41", CultureInfo.InvariantCulture.DateTimeFormat));
    }
}