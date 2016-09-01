using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport.ExportedQuestionTests
{
    public class when_creating_export_structure_for_datetime_question : ExportedQuestionTestContext
    {
        Establish context = () => { };

        Because of = () =>
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.DateTime, new DateTime(2016, 8, 15, 12, 5, 7));
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.DateTime);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.DateTime);
        };

        It should_return_correct_filled_answer = () => filledQuestion.Answers.ShouldEqual(new []{ "2016-08-15" });
        It should_return_correct_disabled_answer = () => disabledQuestion.Answers.ShouldEqual(new []{ DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.Answers.ShouldEqual(new []{ MissingStringQuestionValue });


        private static ExportedQuestion filledQuestion;
        private static ExportedQuestion disabledQuestion;
        private static ExportedQuestion missingQuestion;
    }
}