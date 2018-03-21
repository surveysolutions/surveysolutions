using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_CreatePreloadedDataDtosFromPanelData_is_called_for_2_data_files : PreloadedDataServiceTestContext
    {
        [Test]
        public void Should_not_return_null_result()
        {
            var rosterVariable = "rostergroup";
            var questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(variable: "nq1"),
                    Create.Entity.TextQuestion(variable: "tq1"),
                       Create.Entity.FixedRoster(rosterId: Id.gA,
                        variable: rosterVariable,
                        obsoleteFixedTitles: new[] { "t1", "t2" },
                        children: new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(variable: "nq2")
                        }));

            var importDataParsingService = CreatePreloadedDataService(questionnaireDocument);

            // Act
            var result = importDataParsingService.CreatePreloadedDataDtosFromPanelData(Create.Entity.PreloadedDataByFile(
                CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "nq1" }, new[] { new[] { "1", "2" } }, questionnaireDocument.Title),
                CreatePreloadedDataByFile(new[] { $"{rosterVariable}__id", "nq2", ServiceColumns.InterviewId }, new[] { new[] { "1", "2", "1" } }, rosterVariable)
            ));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Has.Length.EqualTo(1));
        }
    }
}
