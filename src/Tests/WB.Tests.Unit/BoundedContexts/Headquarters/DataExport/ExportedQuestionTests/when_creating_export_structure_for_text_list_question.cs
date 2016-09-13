using Machine.Specifications;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport.ExportedQuestionTests
{
    public class when_creating_export_structure_for_text_list_question : ExportedQuestionTestContext
    {
        Establish context = () => { };

        Because of = () =>
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.TextList, 3, new object[] {"line1", "line2"});
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.TextList, columnsCount: 3);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.TextList, columnsCount: 3);
        };

        It should_return_correct_filled_answer = () => filledQuestion.ShouldEqual(new []{ "line1", "line2", MissingStringQuestionValue });
        It should_return_correct_disabled_answer = () => disabledQuestion.ShouldEqual(new []{ DisableQuestionValue, DisableQuestionValue, DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.ShouldEqual(new []{ MissingStringQuestionValue, MissingStringQuestionValue, MissingStringQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}