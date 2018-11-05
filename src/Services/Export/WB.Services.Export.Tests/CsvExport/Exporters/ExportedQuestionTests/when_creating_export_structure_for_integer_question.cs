using NUnit.Framework;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
{
    public class when_creating_export_structure_for_integer_question : ExportedQuestionTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        public void BecauseOf() 
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.Numeric, 5);
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.Numeric);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.Numeric);
        }

        [NUnit.Framework.Test] public void should_return_correct_filled_answer () => 
            Assert.That(filledQuestion, Is.EquivalentTo(new []{ "5"}));

        [NUnit.Framework.Test] public void should_return_correct_disabled_answer () => 
            Assert.That(disabledQuestion, Is.EquivalentTo(new []{ DisableValue }));

        [NUnit.Framework.Test] public void should_return_correct_missing_answer () => 
            Assert.That(missingQuestion, Is.EquivalentTo(new []{ MissingNumericQuestionValue }));

        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
