using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_with_duplicate_values : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionId = Guid.Parse("21111111111111111111111111111111");
            var question = new MultyOptionsQuestion()
            {
                StataExportCaption = "q1",
                PublicKey = questionId,
                QuestionType = QuestionType.MultyOption,
                Answers =
                    new List<Answer>
                    {
                        new Answer() { AnswerValue = "3", AnswerText = "three" },
                        new Answer() { AnswerValue = "4", AnswerText = "four" }
                    }
            };

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(question);
            questionnaire.VariableName = questionnaire.Title = "questionnaire";

            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "q1_3", "q1_4" },
                new string[][] { new string[] { "1", "3", "3" } },
                "questionnaire.csv");

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire);
        };

        private Because of =
            () =>
                result = preloadedDataVerifier.VerifyPanel(questionnaireId, 1, new[] { preloadedDataByFile });

        private It should_result_has_1_error = () =>
            result.Errors.Count().ShouldEqual(1);

        private It should_return_single_PL0021_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0021");

        private It should_return_reference_with_Column_type = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid questionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}
