using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.DataExportTests.ExportedQuestionTests
{
    public class when_creating_export_structure_for_datetime_question : ExportedQuestionTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        public void BecauseOf() 
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.DateTime, new DateTime(2016, 8, 15, 12, 5, 7));
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.DateTime);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.DateTime);
        }

        [NUnit.Framework.Test] public void should_return_correct_filled_answer () => filledQuestion.Should().BeEquivalentTo("2016-08-15");
        [NUnit.Framework.Test] public void should_return_correct_disabled_answer () => disabledQuestion.Should().BeEquivalentTo(DisableValue);
        [NUnit.Framework.Test] public void should_return_correct_missing_answer () => missingQuestion.Should().BeEquivalentTo(MissingStringQuestionValue);

        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
