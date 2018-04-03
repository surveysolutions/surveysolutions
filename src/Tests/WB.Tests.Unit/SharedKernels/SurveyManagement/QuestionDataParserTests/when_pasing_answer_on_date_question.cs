using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_date_question : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
            BecauseOf();
        }

        public void BecauseOf() 
        {
            parsingResult1 = questionDataParser.TryParse("2016-01-14", questionVarName, dateQuestion, out parcedValue1, out parsedSingleColumnAnswer);
            parsingResult2 = questionDataParser.TryParse("06/26/15 12:01 AM", questionVarName, dateQuestion, out parcedValue2, out parsedSingleColumnAnswer);
            parsingResult3 = questionDataParser.TryParse("06/26/15", questionVarName, dateQuestion, out parcedValue3, out parsedSingleColumnAnswer);
            parsingResult4 = questionDataParser.TryParse("2016-02-02T17:04:41", questionVarName, dateQuestion, out parcedValue4, out parsedSingleColumnAnswer);
            parsingResult5 = questionDataParser.TryParse("2016-02-02", questionVarName, dateQuestion, out parcedValue5, out parsedSingleColumnAnswer);

            parsingResult6 = questionDataParser.TryParse("06/26/15 12:01 AM", questionVarName, currentTimeQuestion, out parcedValue6, out parsedSingleColumnAnswer);
            parsingResult7 = questionDataParser.TryParse("2016-02-02T17:04:41", questionVarName, currentTimeQuestion, out parcedValue7, out parsedSingleColumnAnswer);
        }

        [NUnit.Framework.Test] public void should_result1_be_parsed_successfully () 
        {
            parsingResult1.Should().Be(ValueParsingResult.OK);
            ((DateTime)parcedValue1).Should().Be(new DateTime(2016, 1, 14));
        }

        [NUnit.Framework.Test] public void should_result2_be_parsed_successfully () 
        {
            parsingResult2.Should().Be(ValueParsingResult.OK);
            ((DateTime)parcedValue2).Should().Be(new DateTime(2015, 6, 26, 0, 1, 0));
        }

        [NUnit.Framework.Test] public void should_result3_be_parsed_successfully () 
        {
            parsingResult3.Should().Be(ValueParsingResult.OK);
            ((DateTime)parcedValue3).Should().Be(new DateTime(2015, 6, 26));
        }

        [NUnit.Framework.Test] public void should_result4_be_parsed_successfully () 
        {
            parsingResult4.Should().Be(ValueParsingResult.OK);
            ((DateTime)parcedValue4).Should().Be(new DateTime(2016, 2, 2, 17, 04, 41));
        }

        [NUnit.Framework.Test] public void should_result5_be_parsed_successfully () 
        {
            parsingResult5.Should().Be(ValueParsingResult.OK);
            ((DateTime)parcedValue5).Should().Be(new DateTime(2016, 2, 2));
        }

        [NUnit.Framework.Test] public void should_result6_be_parsed_successfully () 
        {
            parsingResult6.Should().Be(ValueParsingResult.OK);
            ((DateTime)parcedValue6).Should().Be(new DateTime(2015, 6, 26, 0, 1, 0));
        }

        [NUnit.Framework.Test] public void should_result7_be_parsed_successfully () 
        {
            parsingResult7.Should().Be(ValueParsingResult.OK);
            ((DateTime)parcedValue7).Should().Be(new DateTime(2016, 2, 2, 17, 04, 41));
        }

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
