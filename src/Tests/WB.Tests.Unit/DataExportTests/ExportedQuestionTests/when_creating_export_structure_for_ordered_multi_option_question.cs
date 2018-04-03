using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Tests.Unit.DataExportTests.ExportedQuestionTests
{
    public class when_creating_export_structure_for_ordered_multi_option_question : ExportedQuestionTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        public void BecauseOf() 
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.MultyOption, 3, new [] {2, 0}, QuestionSubtype.MultyOption_Ordered);
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.MultyOption, QuestionSubtype.MultyOption_Ordered, columnsCount: 3);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.MultyOption, QuestionSubtype.MultyOption_Ordered, columnsCount: 3);
        }

        [NUnit.Framework.Test] public void should_return_correct_filled_answer () => filledQuestion.Should().BeEquivalentTo(new []{ "2", "0", "1" });
        [NUnit.Framework.Test] public void should_return_correct_disabled_answer () => disabledQuestion.Should().BeEquivalentTo(new []{ DisableValue, DisableValue, DisableValue });
        [NUnit.Framework.Test] public void should_return_correct_missing_answer () => missingQuestion.Should().BeEquivalentTo(new []{ MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
