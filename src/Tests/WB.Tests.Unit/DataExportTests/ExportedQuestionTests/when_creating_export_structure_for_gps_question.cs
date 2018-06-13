using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.DataExportTests.ExportedQuestionTests
{
    public class when_creating_export_structure_for_gps_question : ExportedQuestionTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        public void BecauseOf() 
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.GpsCoordinates, 5, new GeoPosition(1, 2, 3, 4, DateTimeOffset.MinValue));
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.GpsCoordinates, columnsCount: 5);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.GpsCoordinates, columnsCount: 5);
        }

        [NUnit.Framework.Test] public void should_return_correct_filled_answer () => filledQuestion.Should().BeEquivalentTo("1", "2", "3", "4", "0001-01-01T00:00:00");
        [NUnit.Framework.Test] public void should_return_correct_disabled_answer () => disabledQuestion.Should().BeEquivalentTo(DisableValue, DisableValue, DisableValue, DisableValue, DisableValue);
        [NUnit.Framework.Test] public void should_return_correct_missing_answer () => missingQuestion.Should().BeEquivalentTo(MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue, MissingStringQuestionValue);


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
