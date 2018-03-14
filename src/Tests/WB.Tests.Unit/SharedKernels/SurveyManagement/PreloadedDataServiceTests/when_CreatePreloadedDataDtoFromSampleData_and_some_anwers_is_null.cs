using System;
using FluentAssertions;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_CreatePreloadedDataDtoFromSampleData_and_some_anwers_is_null : PreloadedDataServiceTestContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void context()
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion()
                    {
                        StataExportCaption = "nq1",
                        QuestionType = QuestionType.Numeric,
                        PublicKey = Guid.NewGuid()
                    });

            importDataParsingService = CreatePreloadedDataService(questionnaireDocument);
            BecauseOf();
        }

        private void BecauseOf() => exception = Catch.Exception(() => importDataParsingService.CreatePreloadedDataDtoFromAssignmentData(
                CreatePreloadedDataByFile(header: new[] { "Id", "nq1" },
                    content: new[] { new[] { "1", null } },
                    fileName: "some file name")));

        [NUnit.Framework.Test]
        public void should_not_throw_null_reference_exception() =>
            exception.Should().BeNull();

        private static ImportDataParsingService importDataParsingService;
        private static QuestionnaireDocument questionnaireDocument;
        private static Exception exception;
    }
}
