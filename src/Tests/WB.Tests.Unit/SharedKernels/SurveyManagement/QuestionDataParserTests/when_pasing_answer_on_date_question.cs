using System;
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
            questionDataParser = CreateQuestionDataParser();
            dateQuestion = new DateTimeQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = questionVarName,
            };
            currentTimeQuestion = new DateTimeQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = questionVarName,
                IsTimestamp = true,
            };
        };

        Because of = () =>
        {
            parsingResult1 = questionDataParser.TryParse("2016-01-14", questionVarName, dateQuestion, out parcedValue1, out parsedSingleColumnAnswer);
            parsingResult2 = questionDataParser.TryParse("06/26/15 12:01 AM", questionVarName, dateQuestion, out parcedValue2, out parsedSingleColumnAnswer);
            parsingResult3 = questionDataParser.TryParse("06/26/15", questionVarName, dateQuestion, out parcedValue3, out parsedSingleColumnAnswer);
            parsingResult4 = questionDataParser.TryParse("2016-02-02T17:04:41", questionVarName, dateQuestion, out parcedValue4, out parsedSingleColumnAnswer);
            parsingResult5 = questionDataParser.TryParse("2016-02-02", questionVarName, dateQuestion, out parcedValue5, out parsedSingleColumnAnswer);

            parsingResult6 = questionDataParser.TryParse("06/26/15 12:01 AM", questionVarName, currentTimeQuestion, out parcedValue6, out parsedSingleColumnAnswer);
            parsingResult7 = questionDataParser.TryParse("2016-02-02T17:04:41", questionVarName, currentTimeQuestion, out parcedValue7, out parsedSingleColumnAnswer);
        };

        It should_result1_be_parsed_successfully = () =>
        {
            parsingResult1.ShouldEqual(ValueParsingResult.OK);
            ((DateTime)parcedValue1).ShouldEqual(new DateTime(2016, 1, 14));
        };

        It should_result2_be_parsed_successfully = () =>
        {
            parsingResult2.ShouldEqual(ValueParsingResult.OK);
            ((DateTime)parcedValue2).ShouldEqual(new DateTime(2015, 6, 26, 0, 1, 0));
        };

        It should_result3_be_parsed_successfully = () =>
        {
            parsingResult3.ShouldEqual(ValueParsingResult.OK);
            ((DateTime)parcedValue3).ShouldEqual(new DateTime(2015, 6, 26));
        };

        It should_result4_be_parsed_successfully = () =>
        {
            parsingResult4.ShouldEqual(ValueParsingResult.OK);
            ((DateTime)parcedValue4).ShouldEqual(new DateTime(2016, 2, 2, 17, 04, 41));
        };

        It should_result5_be_parsed_successfully = () =>
        {
            parsingResult5.ShouldEqual(ValueParsingResult.OK);
            ((DateTime)parcedValue5).ShouldEqual(new DateTime(2016, 2, 2));
        };

        It should_result6_be_parsed_successfully = () =>
        {
            parsingResult6.ShouldEqual(ValueParsingResult.OK);
            ((DateTime)parcedValue6).ShouldEqual(new DateTime(2015, 6, 26, 0, 1, 0));
        };

        It should_result7_be_parsed_successfully = () =>
        {
            parsingResult7.ShouldEqual(ValueParsingResult.OK);
            ((DateTime)parcedValue7).ShouldEqual(new DateTime(2016, 2, 2, 17, 04, 41));
        };

        protected static DateTimeQuestion dateQuestion;
        protected static DateTimeQuestion currentTimeQuestion;

        protected static ValueParsingResult parsingResult1;
        protected static ValueParsingResult parsingResult2;
        protected static ValueParsingResult parsingResult3;
        protected static ValueParsingResult parsingResult4;
        protected static ValueParsingResult parsingResult5;
        protected static ValueParsingResult parsingResult6;
        protected static ValueParsingResult parsingResult7;

        protected static object parcedValue1;
        protected static object parcedValue2;
        protected static object parcedValue3;
        protected static object parcedValue4;
        protected static object parcedValue5;
        protected static object parcedValue6;
        protected static object parcedValue7;

    }
}