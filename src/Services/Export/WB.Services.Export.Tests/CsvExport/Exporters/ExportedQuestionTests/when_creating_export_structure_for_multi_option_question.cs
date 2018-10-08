using NUnit.Framework;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
{
    public class when_creating_export_structure_for_multi_option_question : ExportedQuestionTestContext
    {
        [OneTimeSetUp]
        public void context()
        {
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.MultyOption, columnsCount: 3);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.MultyOption, columnsCount: 3);
        }

        [Test] public void should_return_correct_disabled_answer () => Assert.That(disabledQuestion, Is.EquivalentTo(new []{ DisableValue, DisableValue, DisableValue }));
        [Test] public void should_return_correct_missing_answer () => Assert.That(missingQuestion, Is.EquivalentTo(new []{ MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue }));

        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
