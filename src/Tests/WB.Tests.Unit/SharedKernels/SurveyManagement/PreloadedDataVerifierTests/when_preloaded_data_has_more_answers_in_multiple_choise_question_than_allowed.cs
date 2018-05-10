using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_preloaded_data_has_more_answers_in_multiple_choise_question_than_allowed : PreloadedDataVerifierTestContext
    {
        [Test]
        public void should_return_verification_error()
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
                    Create.Entity.Answer("three", 3),
                });

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(question);
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(
                new[] { $"{question.VariableName}__1", $"{question.VariableName}__2", $"{question.VariableName}__3" },
                new[] { new[] { "1", "2", "3" } },
                $"{questionnaire.Title}.tab");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);
            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);

            var result = importDataVerifier.VerifyAssignmentsSample(preloadedDataByFile, preloadedDataService).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Single().Code, Is.EqualTo("PL0041"));

            var expectedErrorMessage = string.Format(PreloadingVerificationMessages.PL0041_AnswerExceedsMaxAnswersCount, question.MaxAllowedAnswers);
            Assert.That(result.Single().Message, Is.EqualTo(expectedErrorMessage));
        }
    }
}