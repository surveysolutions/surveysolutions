using Machine.Specifications;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport.ExportedQuestionTests
{
    public class when_creating_export_structure_for_single_option_question : ExportedQuestionTestContext
    {
        Establish context = () => { };

        Because of = () =>
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.SingleOption, "4");
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.SingleOption);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.SingleOption);
        };

        It should_return_correct_filled_answer = () => filledQuestion.ShouldEqual(new []{ "4"});
        It should_return_correct_disabled_answer = () => disabledQuestion.ShouldEqual(new []{ DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.ShouldEqual(new []{ MissingNumericQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}