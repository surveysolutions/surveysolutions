using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_CreatePreloadedDataDtosFromPanelData_and_some_answers_is_null : PreloadedDataServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion() { StataExportCaption = "nq1", QuestionType = QuestionType.Numeric, PublicKey = Guid.NewGuid() },
                    new TextQuestion() { StataExportCaption = "tq1", QuestionType = QuestionType.Text, PublicKey = Guid.NewGuid() },
                       Create.Entity.FixedRoster(rosterId: Guid.NewGuid(),
                        obsoleteFixedTitles: new[] { "t1", "t2" },
                        children: new IComposite[]
                        { new NumericQuestion() { StataExportCaption = "nq2", QuestionType = QuestionType.Numeric, PublicKey = Guid.NewGuid() }}));

            importDataParsingService = CreatePreloadedDataService(questionnaireDocument);
        };

        Because of = () => exception = Catch.Exception(() => importDataParsingService.CreatePreloadedDataDtosFromPanelData(new[]
        {
            CreatePreloadedDataByFile(new[] {ServiceColumns.InterviewId, "nq1"}, new[] {new[] {"1", null}}, questionnaireDocument.Title),
            CreatePreloadedDataByFile(new[] { "rostergroup__id", "nq2", "ParentId1"}, new[] {new[] {"1", null, "1"}}, "rostergroup")
        }));

        It should_not_throw_null_reference_exception = () =>
            exception.ShouldBeNull();

        private static ImportDataParsingService importDataParsingService;
        private static QuestionnaireDocument questionnaireDocument;
        private static Exception exception;
    }
}
