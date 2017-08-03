using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.UI.Headquarters.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests.InterviewImportServiceTests
{
    internal class when_import_interview_and_csv_file_parser_returns_null : InterviewImportServiceTestsContext
    {
        private Establish context = () =>
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter();
            
            var mockOfSamplePreloadingDataParsingService = new Mock<IInterviewImportDataParsingService>();

            mockOfSamplePreloadingDataParsingService.Setup(x => x.GetAssignmentsData("sampleId", questionnaireIdentity, AssignmentImportType.Assignments))
                .Returns((AssignmentImportData[])null);

            interviewImportService =
                CreateInterviewImportService(
                    sampleImportSettings: new SampleImportSettings(1),
                    commandService: mockOfCommandService.Object,
                    interviewImportDataParsingService: mockOfSamplePreloadingDataParsingService.Object, 
                    questionnaireDocument: questionnaireDocument);
        };

        Because of = () => 
            interviewImportService.ImportAssignments(questionnaireIdentity, "sampleId", null, Guid.Parse("22222222222222222222222222222222"), AssignmentImportType.Assignments);

        It should_in_progress_be_false = () =>
            interviewImportService.Status.IsInProgress.ShouldBeFalse();

        It should_have_one_error = () =>
            interviewImportService.Status.State.Errors.Count.ShouldEqual(1);

        It should_have_one_error_with_expected_message = () =>
            interviewImportService.Status.State.Errors.Single().ErrorMessage.ShouldEqual("Datafile is incorrect");

        private static InterviewImportService interviewImportService;
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
    }
}