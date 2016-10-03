using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Export;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport.ExportedQuestionTests
{
    public class when_creating_export_structure_for_yes_no_question : ExportedQuestionTestContext
    {
        Establish context = () => { };

        Because of = () =>
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.MultyOption, 3, new object[2]
            {
                new AnsweredYesNoOption(2, true),
                new AnsweredYesNoOption(0, false),
            }, QuestionSubtype.MultyOption_YesNo);
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.MultyOption, QuestionSubtype.MultyOption_YesNo, columnsCount: 3);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.MultyOption, QuestionSubtype.MultyOption_YesNo, columnsCount: 3);
        };

        It should_return_correct_filled_answer = () => filledQuestion.ShouldEqual(new []{ "0", MissingNumericQuestionValue, "1" });
        It should_return_correct_disabled_answer = () => disabledQuestion.ShouldEqual(new []{ DisableQuestionValue, DisableQuestionValue, DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.ShouldEqual(new []{ MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}