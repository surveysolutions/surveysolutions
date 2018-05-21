using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_multiple_choise_question_is_not_listed_in_preloaded_file: PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_not_validate_max_answer_count()
        {
            var questionnaireId = Id.g1;
            var questionId = Id.gA;
            var question = Create.Entity.MultyOptionsQuestion(questionId,
                variable: "mul",
                maxAllowedAnswers: 2,
                options: new List<Answer>
                {
                    Create.Entity.Answer("one", 1),
                    Create.Entity.Answer("two", 2),
                    Create.Entity.Answer("three", 3)
                });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(question, Create.Entity.NumericIntegerQuestion(Id.g2, variable: "numeric1"));
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(
                new[] { "numeric1" },
                new[] { new[] { "5" } },
                $"{questionnaire.Title}.tab");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);
            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);

            var result = importDataVerifier.VerifyAssignmentsSample(preloadedDataByFile, preloadedDataService).ToList();

            Assert.That(result, Has.Count.EqualTo(0));
        }
    }
}