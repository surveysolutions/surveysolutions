using System;
using NUnit.Framework;
using WB.Services.Export.Questionnaire;

namespace WB.Services.Export.Tests.CsvExport.Exporters.ExportedQuestionTests
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

        [NUnit.Framework.Test] public void should_return_correct_filled_answer () => Assert.That(filledQuestion, Is.EquivalentTo(new []{"2016-08-15"}));
        [NUnit.Framework.Test] public void should_return_correct_disabled_answer () => Assert.That(disabledQuestion, Is.EquivalentTo(new []{DisableValue}));
        [NUnit.Framework.Test] public void should_return_correct_missing_answer () => Assert.That(missingQuestion, Is.EquivalentTo(new []{MissingStringQuestionValue}));

        private static string[] filledQuestion;
        private static string[] disabledQuestion;
        private static string[] missingQuestion;
    }
}
