using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_CreatePreloadedDataDtoFromSampleData_and_some_anwers_is_null : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion()
                    {
                        StataExportCaption = "nq1",
                        QuestionType = QuestionType.Numeric,
                        PublicKey = Guid.NewGuid()
                    });

            preloadedDataService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of = () => exception = Catch.Exception(() => preloadedDataService.CreatePreloadedDataDtoFromSampleData(
                CreatePreloadedDataByFile(header: new[] {"Id", "nq1"},
                    content: new[] {new[] {"1", null}},
                    fileName: "some file name")));

        It should_not_throw_null_reference_exception = () =>
            exception.ShouldBeNull();

        private static PreloadedDataService preloadedDataService;
        private static QuestionnaireDocument questionnaireDocument;
        private static Exception exception;
    }
}
