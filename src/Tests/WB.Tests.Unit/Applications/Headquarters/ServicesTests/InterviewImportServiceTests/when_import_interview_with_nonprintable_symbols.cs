using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.SampleImport;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;


namespace WB.Tests.Unit.Applications.Headquarters.ServicesTests.InterviewImportServiceTests
{
    internal class when_import_interview_with_nonprintable_symbols : InterviewImportServiceTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireDocument=
                Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericQuestion(questionId: Guid.Parse("33333333333333333333333333333333"), variableName: "EANo", prefilled: true, isInteger: true),
                    Create.Entity.NumericQuestion(questionId: Guid.Parse("44444444444444444444444444444444"), variableName: "MapRefNo", prefilled: true, isInteger: true),
                    Create.Entity.NumericQuestion(questionId: Guid.Parse("55555555555555555555555555555555"), variableName: "DUNo", prefilled: true, isInteger: true),
                    Create.Entity.TextQuestion(questionId: Guid.Parse("66666666666666666666666666666666"), variable: "Prov", preFilled: true),
                    Create.Entity.TextQuestion(questionId: Guid.Parse("77777777777777777777777777777777"), variable: "LocalMunic", preFilled: true),
                    Create.Entity.TextQuestion(questionId: Guid.Parse("88888888888888888888888888888888"), variable: "MainPlace", preFilled: true),
                    Create.Entity.TextQuestion(questionId: Guid.Parse("99999999999999999999999999999999"), variable: "SubPlace", preFilled: true),
                    Create.Entity.GpsCoordinateQuestion(questionId: Guid.Parse("10101010101010101010101010101010"), variable: "LongLat", isPrefilled: true));
            
            var mockOfSamplePreloadingDataParsingService = new Mock<IInterviewImportDataParsingService>();
            mockOfSamplePreloadingDataParsingService.Setup(x => x.GetAssignmentsData("sampleId", questionnaireIdentity, AssignmentImportType.Assignments))
                .Returns(new[]
                {
                    new AssignmentImportData
                    {
                        InterviewerId = interviewerId,
                        SupervisorId = supervisorId,
                        PreloadedData = new PreloadedDataDto(new [] { new PreloadedLevelDto(new decimal[0], new Dictionary<Guid, AbstractAnswer>()) })
                    }
                });

            mockOfCommandService.Setup(x => x.Execute(Moq.It.IsAny<CreateInterview>(), null)).Callback<ICommand, string>(
                    (command, ordinal) =>
                    {
                        executedCommand = command as CreateInterview;
                    });

            var questionnaireBrowseItem = Create.Entity.QuestionnaireBrowseItem(questionnaireDocument, false);
            var questionnaireFactory = Mock.Of<IQuestionnaireBrowseViewFactory>(x => x.GetById(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaireBrowseItem);

            interviewImportService =
                CreateInterviewImportService(
                    sampleImportSettings: new SampleImportSettings(1),
                    commandService: mockOfCommandService.Object,
                    interviewImportDataParsingService: mockOfSamplePreloadingDataParsingService.Object, 
                    questionnaireDocument: questionnaireDocument,
                    questionnaireBrowseViewFactory: questionnaireFactory);
            BecauseOf();
        }

        public void BecauseOf() => interviewImportService.ImportAssignments(questionnaireIdentity, "sampleId", null, Guid.Parse("22222222222222222222222222222222"), AssignmentImportType.Assignments, false);

        [NUnit.Framework.Test] public void should_call_execute_command_service_once () =>
            mockOfCommandService.Verify(x=> x.Execute(Moq.It.IsAny<CreateInterview>(), null), Times.Once);

        [NUnit.Framework.Test] public void should_be_specified_interviewer () =>
            executedCommand.InterviewerId.Should().Be(interviewerId);

        [NUnit.Framework.Test] public void should_be_specified_supervisor () =>
            executedCommand.SupervisorId.Should().Be(supervisorId);

        //private static readonly byte[] csvBytes = Encoding.UTF8.GetBytes(
        //    "Responsible	EANo	MapRefNo	DUNo	Prov	LocalMunic	MainPlace	SubPlace	LongLat__Latitude	LongLat__Longitude\r\n" +
        //    @"GONZALES	138215891	318	2513	<?=/)L62O]#)7P#I_JOG[;>)1'	;A)=1C9'82LQ+K-S;YJ`AR	OR	`^!!4_!\\QF@RG_HL73ZD\	-6	1");

        private static CreateInterview executedCommand = null;
        private static InterviewImportService interviewImportService;
        private static readonly Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static readonly Guid supervisorId = Guid.Parse("13131313131313131313131313131313");
        private static readonly Guid interviewerId = Guid.Parse("12121212121212121212121212121212");
    }
}
