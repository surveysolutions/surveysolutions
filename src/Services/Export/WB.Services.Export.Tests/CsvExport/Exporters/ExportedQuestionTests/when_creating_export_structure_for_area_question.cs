using System;
using NUnit.Framework;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
{
    public class when_creating_export_structure_for_area_question : ExportedQuestionTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        public void BecauseOf()
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.Area, 6, new Area("json", "mapName", 1, 2, 3, "coordiantes", 5, 6, 7));
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.Area, columnsCount: 6);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.Area, columnsCount: 6);
        }

        [NUnit.Framework.Test] public void should_return_correct_filled_answer() => 
            Assert.That(filledQuestion, Is.EquivalentTo(new[] { "coordiantes", "2", "3", "1", "6", "7" }));

        [NUnit.Framework.Test] public void should_return_correct_disabled_answer() => 
            Assert.That(disabledQuestion, Is.EquivalentTo(new[] { DisableValue, DisableValue, DisableValue, DisableValue, DisableValue, DisableValue }));

        [NUnit.Framework.Test] public void should_return_correct_missing_answer() => 
            Assert.That(missingQuestion, 
                Is.EquivalentTo(new[] { MissingStringQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue }));


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
