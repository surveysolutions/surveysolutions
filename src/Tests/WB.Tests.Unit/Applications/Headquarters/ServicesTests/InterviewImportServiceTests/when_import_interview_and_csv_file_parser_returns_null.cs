using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests.InterviewImportServiceTests
{
    internal class when_import_interview_and_csv_file_parser_returns_null : InterviewImportServiceTestsContext
    {
        private Establish context = () =>
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter();
            
            var mockOfPreloadedDataRepository = new Mock<IPreloadedDataRepository>();
            var mockOfSamplePreloadingDataParsingService = new Mock<IInterviewImportDataParsingService>();

            mockOfSamplePreloadingDataParsingService.Setup(x => x.GetInterviewsImportData("sampleId", questionnaireIdentity))
                .Returns((InterviewImportData[])null);

            interviewImportService =
                CreateInterviewImportService(
                    sampleImportSettings: new SampleImportSettings(1),
                    commandService: mockOfCommandService.Object,
                    preloadedDataRepository: mockOfPreloadedDataRepository.Object,
                    interviewImportDataParsingService: mockOfSamplePreloadingDataParsingService.Object, 
                    questionnaireDocument: questionnaireDocument);
        };

        Because of = () => 
            interviewImportService.ImportInterviews(questionnaireIdentity, "sampleId", null, Guid.Parse("22222222222222222222222222222222"));

        It should_in_progress_be_false = () =>
            interviewImportService.Status.IsInProgress.ShouldBeFalse();

        It should_call_execute_command_service_once = () =>
            interviewImportService.Status.State.Errors.Count.ShouldEqual(1);

        private static InterviewImportService interviewImportService;
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
    }
}