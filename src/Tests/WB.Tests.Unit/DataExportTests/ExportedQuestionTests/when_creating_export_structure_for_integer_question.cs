using FluentAssertions;
using Main.Core.Entities.SubEntities;

namespace WB.Tests.Unit.DataExportTests.ExportedQuestionTests
{
    public class when_creating_export_structure_for_integer_question : ExportedQuestionTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        public void BecauseOf() 
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.Numeric, 5);
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.Numeric);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.Numeric);
        }

        [NUnit.Framework.Test] public void should_return_correct_filled_answer () => filledQuestion.Should().BeEquivalentTo(new []{ "5"});
        [NUnit.Framework.Test] public void should_return_correct_disabled_answer () => disabledQuestion.Should().BeEquivalentTo(new []{ DisableValue });
        [NUnit.Framework.Test] public void should_return_correct_missing_answer () => missingQuestion.Should().BeEquivalentTo(new []{ MissingNumericQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
