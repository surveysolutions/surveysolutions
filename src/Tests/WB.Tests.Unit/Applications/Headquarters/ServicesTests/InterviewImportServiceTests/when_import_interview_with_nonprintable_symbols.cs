using System;
using System.Collections.Generic;
using System.Text;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.SampleImport;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.UI.Headquarters.Implementation.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests.InterviewImportServiceTests
{
    internal class when_import_interview_with_nonprintable_symbols : InterviewImportServiceTestsContext
    {
        private Establish context = () =>
        {
            var questionnaireRepository = Create.CreateQuestionnaireReadSideKeyValueStorage(
                Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericQuestion(questionId: Guid.Parse("33333333333333333333333333333333"), variableName: "EANo", prefilled: true, isInteger: true),
                    Create.NumericQuestion(questionId: Guid.Parse("44444444444444444444444444444444"), variableName: "MapRefNo", prefilled: true, isInteger: true),
                    Create.NumericQuestion(questionId: Guid.Parse("55555555555555555555555555555555"), variableName: "DUNo", prefilled: true, isInteger: true),
                    Create.TextQuestion(questionId: Guid.Parse("66666666666666666666666666666666"), variable: "Prov", preFilled: true),
                    Create.TextQuestion(questionId: Guid.Parse("77777777777777777777777777777777"), variable: "LocalMunic", preFilled: true),
                    Create.TextQuestion(questionId: Guid.Parse("88888888888888888888888888888888"), variable: "MainPlace", preFilled: true),
                    Create.TextQuestion(questionId: Guid.Parse("99999999999999999999999999999999"), variable: "SubPlace", preFilled: true),
                    Create.GpsCoordinateQuestion(questionId: Guid.Parse("10101010101010101010101010101010"), variable: "LongLat", isPrefilled: true)));
            
            var mockOfPreloadedDataRepository = new Mock<IPreloadedDataRepository>();
            mockOfPreloadedDataRepository.Setup(x => x.GetBytesOfSampleData(Moq.It.IsAny<string>())).Returns(csvBytes);

            var mockOfSamplePreloadingDataParsingService = new Mock<IInterviewImportDataParsingService>();
            mockOfSamplePreloadingDataParsingService.Setup(x => x.GetInterviewsImportData("sampleId", questionnaireIdentity))
                .Returns(new[]
                {
                    new InterviewImportData()
                    {
                        InterviewerId = interviewerId,
                        SupervisorId = supervisorId,
                        Answers = new Dictionary<Guid, object>()
                    }
                });

            mockOfCommandService.Setup(x => x.Execute(Moq.It.IsAny<CreateInterviewByPrefilledQuestions>(), null)).Callback<ICommand, string>(
                    (command, ordinal) =>
                    {
                        executedCommand = command as CreateInterviewByPrefilledQuestions;
                    });

            interviewImportService =
                CreateInterviewImportService(questionnaireDocumentRepository: questionnaireRepository,
                    sampleImportSettings: new SampleImportSettings(1),
                    commandService: mockOfCommandService.Object,
                    preloadedDataRepository: mockOfPreloadedDataRepository.Object,
                    interviewImportDataParsingService: mockOfSamplePreloadingDataParsingService.Object);
        };

        Because of = () => exception = Catch.Exception(() =>
                interviewImportService.ImportInterviews(questionnaireIdentity, "sampleId", null, Guid.Parse("22222222222222222222222222222222")));

        It should_not_be_exception = () =>
            exception.ShouldBeNull();

        It should_call_execute_command_service_once = () =>
            mockOfCommandService.Verify(x=> x.Execute(Moq.It.IsAny<CreateInterviewByPrefilledQuestions>(), null), Times.Once);

        It should_be_specified_interviewer = () =>
            executedCommand.InterviewerId.ShouldEqual(interviewerId);

        It should_be_specified_supervisor = () =>
            executedCommand.SupervisorId.ShouldEqual(supervisorId);

        private static readonly byte[] csvBytes = Encoding.UTF8.GetBytes(
            "Responsible	EANo	MapRefNo	DUNo	Prov	LocalMunic	MainPlace	SubPlace	LongLat__Latitude	LongLat__Longitude\r\n" +
            @"GONZALES	138215891	318	2513	<?=/)L62O]#)7P#I_JOG[;>)1'	;A)=1C9'82LQ+K-S;YJ`AR	OR	`^!!4_!\\QF@RG_HL73ZD\	-6	1");

        private static CreateInterviewByPrefilledQuestions executedCommand = null;
        private static Exception exception;
        private static InterviewImportService interviewImportService;
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static readonly Guid longlatId = Guid.Parse("10101010101010101010101010101010");
        private static readonly Guid subplaceId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid mainplaceId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid localmunicId = Guid.Parse("77777777777777777777777777777777");
        private static readonly Guid provId = Guid.Parse("66666666666666666666666666666666");
        private static readonly Guid dunoId = Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid maprefnoId = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid eanoId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid headquartersId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid supervisorId = Guid.Parse("13131313131313131313131313131313");
        private static readonly Guid interviewerId = Guid.Parse("12121212121212121212121212121212");
    }
}