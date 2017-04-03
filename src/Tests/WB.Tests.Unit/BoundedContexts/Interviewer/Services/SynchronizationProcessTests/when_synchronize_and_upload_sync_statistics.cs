using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Tests.Abc;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    internal class when_synchronize_and_upload_sync_statistics : SynchronizationProcessTestsContext
    {
        [Test]
        public async Task counter_by_new_device_interviews_should_not_include_census()
        {
            var principal = Setup.InterviewerPrincipal("name", "pass");

            var interviewId = Guid.NewGuid();

            var questionnaireIdentity = new QuestionnaireIdentity(
                Guid.Parse("11111111111111111111111111111111"), 1);
            
            var censusInterview = new InterviewView
            {
                Id = interviewId.ToString(),
                InterviewId = interviewId,
                CanBeDeleted = true,
                Status = InterviewStatus.InterviewerAssigned,
                QuestionnaireId = questionnaireIdentity.ToString()
            };

            var interviewViewRepository = new SqliteInmemoryStorage<InterviewView>();
            interviewViewRepository.Store(new List<InterviewView>
            {
                censusInterview
            });

            var synchronizationServiceMock = new Mock<ISynchronizationService>();
            synchronizationServiceMock.Setup(x => x.GetCensusQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QuestionnaireIdentity>(new[] {questionnaireIdentity}));
            synchronizationServiceMock.Setup(x => x.GetServerQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QuestionnaireIdentity>(new[] { questionnaireIdentity }));
            synchronizationServiceMock.Setup(x => x.GetInterviewsAsync(Moq.It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InterviewApiView>());

            var interviewerQuestionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(
                x => x.GetCensusQuestionnaireIdentities() == new List<QuestionnaireIdentity>(new[] { questionnaireIdentity })
                     && x.GetAllQuestionnaireIdentities() == new List<QuestionnaireIdentity>(new[] { questionnaireIdentity })
            );

            var mockOFInterviewAccessor = new Mock<IInterviewerInterviewAccessor>();

            var viewModel = CreateSynchronizationProcess(principal: principal,
                interviewViewRepository: interviewViewRepository,
                synchronizationService: synchronizationServiceMock.Object,
                questionnaireFactory: interviewerQuestionnaireAccessor,
                interviewFactory: mockOFInterviewAccessor.Object
            );

            await viewModel.SyncronizeAsync(Mock.Of<IProgress<SyncProgressInfo>>(), CancellationToken.None);

            synchronizationServiceMock.Verify(x =>
                x.SendSyncStatisticsAsync(Moq.It.Is<SyncStatisticsApiView>(y=>y.NewInterviewsOnDeviceCount == 0), Moq.It.IsAny<CancellationToken>(),
                    Moq.It.IsAny<RestCredentials>()), Times.Once);
        }
    }
}
