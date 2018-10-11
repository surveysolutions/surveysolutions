using NUnit.Framework;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
{
    public class when_creating_export_structure_for_answered_multi_option_question : ExportedQuestionTestContext
    {
        [Test]
        public void should_return_correct_filled_answer()
        {
            var filledQuestion = CreateFilledExportedQuestion(QuestionType.MultyOption, 3, new [] {2, 0});
            Assert.That(filledQuestion, Is.EquivalentTo(new []{ "1", "0", "1" }));
        }
    }
}