using Machine.Specifications;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport.ExportedQuestionTests
{
    public class when_creating_export_structure_for_multimedia_question : ExportedQuestionTestContext
    {
        Establish context = () => { };

        Because of = () =>
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.Multimedia, "image file");
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.Multimedia);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.Multimedia);
        };

        It should_return_correct_filled_answer = () => filledQuestion.ShouldEqual(new []{ "image file" });
        It should_return_correct_disabled_answer = () => disabledQuestion.ShouldEqual(new []{ DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.ShouldEqual(new []{ MissingStringQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}