using Machine.Specifications;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport.ExportedQuestionTests
{
    public class when_creating_export_structure_for_qr_barcode_question : ExportedQuestionTestContext
    {
        Establish context = () => { };

        Because of = () =>
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.QRBarcode, "qr bar code");
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.QRBarcode);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.QRBarcode);
        };

        It should_return_correct_filled_answer = () => filledQuestion.ShouldEqual(new []{ "qr bar code" });
        It should_return_correct_disabled_answer = () => disabledQuestion.ShouldEqual(new []{ DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.ShouldEqual(new []{ MissingStringQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}