using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

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

        It should_return_correct_filled_answer = () => filledQuestion.Answers.ShouldEqual(new []{ "4"});
        It should_return_correct_disabled_answer = () => disabledQuestion.Answers.ShouldEqual(new []{ DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.Answers.ShouldEqual(new []{ MissingNumericQuestionValue });


        private static ExportedQuestion filledQuestion;
        private static ExportedQuestion disabledQuestion;
        private static ExportedQuestion missingQuestion;
    }
}