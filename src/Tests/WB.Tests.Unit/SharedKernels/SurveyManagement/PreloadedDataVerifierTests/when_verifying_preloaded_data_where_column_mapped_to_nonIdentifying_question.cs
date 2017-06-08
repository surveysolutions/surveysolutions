using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_sample_where_column_mapped_to_nonIdentifying_question : PreloadedDataVerifierTestContext
    {
        [OneTimeSetUp]
        public void Context()
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            numericQuestionId = Guid.Parse("21111111111111111111111111111111");

            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(chapterChildren:
                    new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(numericQuestionId, "num", isPrefilled: false)
                    });

            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "num" }, new[] { new[] { "3" } }, "questionnaire.csv");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);
            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);

            this.Because();
        }

        private void Because() => result = preloadedDataVerifier.VerifyAssignmentsSample(questionnaireId, 1, preloadedDataByFile);

        [Test]
        public void should_result_has_1_error() => result.Errors.Count().ShouldEqual(1);

        [Test]
        public void should_return_single_PL0037_error() => result.Errors.First().Code.ShouldEqual("PL0037");

        [Test]
        public void should_return_reference_with_Cell_type() =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Column);

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid numericQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}