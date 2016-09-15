using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport.ExportedQuestionTests
{
    public class when_creating_export_structure_for_time_question : ExportedQuestionTestContext
    {
        Establish context = () => { };

        Because of = () =>
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.DateTime, new DateTime(2016, 8, 15, 12, 5, 7), QuestionSubtype.DateTime_Timestamp);
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.DateTime, QuestionSubtype.DateTime_Timestamp);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.DateTime, QuestionSubtype.DateTime_Timestamp);
        };

        It should_return_correct_filled_answer = () => filledQuestion.ShouldEqual(new []{ "2016-08-15T12:05:07" });
        It should_return_correct_disabled_answer = () => disabledQuestion.ShouldEqual(new []{ DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.ShouldEqual(new []{ MissingStringQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}