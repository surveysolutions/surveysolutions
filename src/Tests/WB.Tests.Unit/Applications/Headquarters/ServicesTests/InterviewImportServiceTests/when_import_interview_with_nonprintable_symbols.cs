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
            var mockOfSampleUploadVievFactory = new Mock<IViewFactory<SampleUploadViewInputModel, SampleUploadView>>();
            mockOfSampleUploadVievFactory.Setup(x => x.Load(Moq.It.IsAny<SampleUploadViewInputModel>()))
                .Returns(new SampleUploadView(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version,
                    new List<FeaturedQuestionItem>()
                    {
                        new FeaturedQuestionItem(Guid.Parse("33333333333333333333333333333333"), "", "EANo"),
                        new FeaturedQuestionItem(Guid.Parse("44444444444444444444444444444444"), "", "MapRefNo"),
                        new FeaturedQuestionItem(Guid.Parse("55555555555555555555555555555555"), "", "DUNo"),
                        new FeaturedQuestionItem(Guid.Parse("66666666666666666666666666666666"), "", "Prov"),
                        new FeaturedQuestionItem(Guid.Parse("77777777777777777777777777777777"), "", "LocalMunic"),
                        new FeaturedQuestionItem(Guid.Parse("88888888888888888888888888888888"), "", "MainPlace"),
                        new FeaturedQuestionItem(Guid.Parse("99999999999999999999999999999999"), "", "SubPlace"),
                        new FeaturedQuestionItem(Guid.Parse("10101010101010101010101010101010"), "", "LongLat__Latitude"),
                        new FeaturedQuestionItem(Guid.Parse("10101010101010101010101010101010"), "", "LongLat__Longitude")
                    }));

            var questionnaireRepository = Create.CreateQuestionnaireReadSideKeyValueStorage(
                Create.QuestionnaireDocumentWithOneChapter(
                    Create.NumericQuestion(questionId: Guid.Parse("33333333333333333333333333333333"), variableName: "EANo", prefilled: true, isInteger: true),
                    Create.NumericQuestion(questionId: Guid.Parse("44444444444444444444444444444444"), variableName: "MapRefNo", prefilled: true, isInteger: true),
                    Create.NumericQuestion(questionId: Guid.Parse("55555555555555555555555555555555"), variableName: "DUNo", prefilled: true, isInteger: true),
                    Create.TextQuestion(questionId: Guid.Parse("66666666666666666666666666666666"), variable: "Prov", preFilled: true),
                    Create.TextQuestion(questionId: Guid.Parse("77777777777777777777777777777777"), variable: "LocalMunic", preFilled: true),
                    Create.TextQuestion(questionId: Guid.Parse("88888888888888888888888888888888"), variable: "MainPlace", preFilled: true),
                    Create.TextQuestion(questionId: Guid.Parse("99999999999999999999999999999999"), variable: "SubPlace", preFilled: true),
                    Create.GpsCoordinateQuestion(questionId: Guid.Parse("10101010101010101010101010101010"), variableName: "LongLat", isPrefilled: true)));
            
            var mockOfPreloadedDataRepository = new Mock<IPreloadedDataRepository>();
            mockOfPreloadedDataRepository.Setup(x => x.GetBytesOfSampleData(Moq.It.IsAny<string>())).Returns(csvBytes);

            var mockOfSamplePreloadingDataParsingService = new Mock<ISamplePreloadingDataParsingService>();
            mockOfSamplePreloadingDataParsingService.Setup(x => x.ParseSample("sampleId", questionnaireIdentity))
                .Returns(new[]
                {
                    new InterviewSampleData()
                    {
                        InterviewerId = Guid.NewGuid(),
                        SupervisorId = Guid.NewGuid(),
                        Answers = new Dictionary<Guid, object>()
                    }
                });

            interviewImportService =
                CreateInterviewImportService(questionnaireDocumentRepository: questionnaireRepository,
                    sampleImportSettings: new SampleImportSettings(1),
                    commandService: mockOfCommandService.Object,
                    preloadedDataRepository: mockOfPreloadedDataRepository.Object,
                    samplePreloadingDataParsingService: mockOfSamplePreloadingDataParsingService.Object);
        };

        Because of = () => exception = Catch.Exception(() =>
                interviewImportService.ImportInterviews(questionnaireIdentity, "sampleId", null, Guid.Parse("22222222222222222222222222222222")));

        It should_not_be_exception = () =>
            exception.ShouldBeNull();

        It should_call_execute_command_service_once = () =>
            mockOfCommandService.Verify(x=> x.Execute(Moq.It.IsAny<CreateInterviewByPrefilledQuestions>(), null), Times.Once);

        private static readonly byte[] csvBytes = Encoding.UTF8.GetBytes(
            "Responsible	EANo	MapRefNo	DUNo	Prov	LocalMunic	MainPlace	SubPlace	LongLat__Latitude	LongLat__Longitude\r\n" +
            @"GONZALES	138215891	318	2513	<?=/)L62O]#)7P#I_JOG[;>)1'	;A)=1C9'82LQ+K-S;YJ`AR	OR	`^!!4_!\\QF@RG_HL73ZD\	-6	1");

        private static Exception exception;
        private static Mock<ICommandService> mockOfCommandService = new Mock<ICommandService>();
        private static readonly QuestionnaireIdentity questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111111111111111111111111111"), 1);
        private static InterviewImportService interviewImportService;
    }
}