using FluentAssertions;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
{
    public class when_creating_export_structure_for_yes_no_question : ExportedQuestionTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            BecauseOf();
        }

        public void BecauseOf() 
        {
            filledQuestion = CreateFilledExportedQuestion(QuestionType.MultyOption, 3, new AnsweredYesNoOption[2]
            {
                new AnsweredYesNoOption(2, true),
                new AnsweredYesNoOption(0, false),
            }, QuestionSubtype.MultyOption_YesNo);
            disabledQuestion = CreateDisabledExportedQuestion(QuestionType.MultyOption, QuestionSubtype.MultyOption_YesNo, columnsCount: 3);
            missingQuestion = CreateMissingValueExportedQuestion(QuestionType.MultyOption, QuestionSubtype.MultyOption_YesNo, columnsCount: 3);
        }

        [NUnit.Framework.Test] public void should_return_correct_filled_answer () => filledQuestion.Should().BeEquivalentTo(new []{ "0", MissingNumericQuestionValue, "1" });
        [NUnit.Framework.Test] public void should_return_correct_disabled_answer () => disabledQuestion.Should().BeEquivalentTo(new []{ DisableValue, DisableValue, DisableValue });
        [NUnit.Framework.Test] public void should_return_correct_missing_answer () => missingQuestion.Should().BeEquivalentTo(new []{ MissingNumericQuestionValue, MissingNumericQuestionValue, MissingNumericQuestionValue });


        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
