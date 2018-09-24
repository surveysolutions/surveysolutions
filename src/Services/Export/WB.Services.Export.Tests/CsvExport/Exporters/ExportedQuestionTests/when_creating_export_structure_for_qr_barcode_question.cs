using FluentAssertions;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
{
    public class when_creating_export_structure_for_qr_barcode_question : ExportedQuestionTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        public void BecauseOf() 
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.QRBarcode, "qr bar code");
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.QRBarcode);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.QRBarcode);
        }

        [NUnit.Framework.Test] public void should_return_correct_filled_answer () => filledQuestion.Should().BeEquivalentTo(new []{ "qr bar code" });
        [NUnit.Framework.Test] public void should_return_correct_disabled_answer () => disabledQuestion.Should().BeEquivalentTo(new []{ DisableValue });
        [NUnit.Framework.Test] public void should_return_correct_missing_answer () => missingQuestion.Should().BeEquivalentTo(new []{ MissingStringQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
