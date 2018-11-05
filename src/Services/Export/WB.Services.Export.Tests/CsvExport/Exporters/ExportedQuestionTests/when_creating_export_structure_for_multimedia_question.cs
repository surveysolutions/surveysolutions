using NUnit.Framework;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
{
    public class when_creating_export_structure_for_multimedia_question : ExportedQuestionTestContext
    {
        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;

        [OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        private void BecauseOf()
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.Multimedia, "image file");
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.Multimedia);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.Multimedia);
        }

        [Test]
        public void should_return_correct_filled_answer()
        {
            Assert.That(filledQuestion, Is.EquivalentTo(new[] {"image file"}));
        }

        [Test]
        public void should_return_correct_disabled_answer()
        {
            Assert.That(disabledQuestion, Is.EquivalentTo(new[] {DisableValue}));
        }

        [Test]
        public void should_return_correct_missing_answer()
        {
            Assert.That(missingQuestion, Is.EquivalentTo(new[] {MissingStringQuestionValue}));
        }
    }
}
