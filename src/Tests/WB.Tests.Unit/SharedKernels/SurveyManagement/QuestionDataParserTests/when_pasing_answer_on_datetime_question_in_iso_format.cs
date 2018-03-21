using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionDataParserTests
{
    internal class when_pasing_answer_on_datetime_question_in_iso_format : QuestionDataParserTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDataParser = CreateQuestionDataParser();
            question = new DateTimeQuestion()
            {
                PublicKey = questionId,
                QuestionType = QuestionType.DateTime,
                StataExportCaption = questionVarName,
                IsTimestamp = true
            };
            BecauseOf();
        }

        public void BecauseOf() => 
            parsingResult = questionDataParser.TryParse(expectedDateTimeAnswer.ToString(ExportFormatSettings.ExportDateTimeFormat), questionVarName, question, out parcedValue, out parsedSingleColumnAnswer);

        [NUnit.Framework.Test] public void should_result_be_equal_to_epexted () =>
            parcedValue.Should().Be(expectedDateTimeAnswer);

        private static readonly DateTime expectedDateTimeAnswer = new DateTime(2016, 02, 13, 17, 04, 41);
    }
}
