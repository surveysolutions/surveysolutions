using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

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

        It should_return_correct_filled_answer = () => filledQuestion.Answers.ShouldEqual(new []{ "line1", "line2", MissingStringQuestionValue });
        It should_return_correct_disabled_answer = () => disabledQuestion.Answers.ShouldEqual(new []{ DisableQuestionValue, DisableQuestionValue, DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.Answers.ShouldEqual(new []{ MissingStringQuestionValue, MissingStringQuestionValue, MissingStringQuestionValue });


        private static ExportedQuestion filledQuestion;
        private static ExportedQuestion disabledQuestion;
        private static ExportedQuestion missingQuestion;
    }
}