using System;
using FluentAssertions;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
{
    public class when_creating_export_structure_for_time_question : ExportedQuestionTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        public void BecauseOf() 
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.DateTime, new DateTime(2016, 8, 15, 12, 5, 7), QuestionSubtype.DateTime_Timestamp);
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.DateTime, QuestionSubtype.DateTime_Timestamp);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.DateTime, QuestionSubtype.DateTime_Timestamp);
        }

        [NUnit.Framework.Test] public void should_return_correct_filled_answer ()
        {
            filledQuestion.Should().BeEquivalentTo(new[] {"2016-08-15T12:05:07"});
        }

        [NUnit.Framework.Test] public void should_return_correct_disabled_answer ()
        {
            disabledQuestion.Should().BeEquivalentTo(new[] {DisableValue});
        }

        [NUnit.Framework.Test] public void should_return_correct_missing_answer ()
        {
            missingQuestion.Should().BeEquivalentTo(new[] {MissingStringQuestionValue});
        }


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
