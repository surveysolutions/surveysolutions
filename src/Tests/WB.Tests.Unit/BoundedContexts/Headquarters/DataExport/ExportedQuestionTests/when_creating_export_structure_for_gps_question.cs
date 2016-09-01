using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.DataExport.ExportedQuestionTests
{
    public class when_creating_export_structure_for_gps_question : ExportedQuestionTestContext
    {
        Establish context = () => { };

        Because of = () =>
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.GpsCoordinates, 5, new GeoPosition(1, 2, 3, 4, DateTimeOffset.MinValue));
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.GpsCoordinates, columnsCount: 5);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.GpsCoordinates, columnsCount: 5);
        };

        It should_return_correct_filled_answer = () => filledQuestion.Answers.ShouldEqual(new []{ "1", "2", "3", "4", "0001-01-01T00:00:00" });
        It should_return_correct_disabled_answer = () => disabledQuestion.Answers.ShouldEqual(new []{ DisableQuestionValue, DisableQuestionValue, DisableQuestionValue, DisableQuestionValue, DisableQuestionValue });
        It should_return_correct_missing_answer = () => missingQuestion.Answers.ShouldEqual(new []{ MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue, MissingStringQuestionValue });


        private static ExportedQuestion filledQuestion;
        private static ExportedQuestion disabledQuestion;
        private static ExportedQuestion missingQuestion;
    }
}