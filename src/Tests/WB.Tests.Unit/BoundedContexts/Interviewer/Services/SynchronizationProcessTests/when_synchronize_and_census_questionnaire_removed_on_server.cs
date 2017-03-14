using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Tests.Abc;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    internal class when_synchronize_and_census_questionnaire_removed_on_server : SynchronizationProcessTestsContext
    {
        [Test]
        public async Task should_progress_report_1_deleted_interview()
        {
            var principal = Setup.InterviewerPrincipal("name", "pass");

            var interviewId = Guid.NewGuid();
            int totalDeletedInterviewsCount = 0;

            var interviewViewRepository = new SqliteInmemoryStorage<InterviewView>();
            interviewViewRepository.Store(new List<InterviewView>
            {
                new InterviewView
                {
                    Id = interviewId.ToString(),
                    InterviewId = interviewId,
                    CanBeDeleted = true,
                    QuestionnaireId = "questionnaire id"
                }
            });

            var synchronizationService = Mock.Of<ISynchronizationService>(
                x =>
                    x.GetCensusQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) ==
                    Task.FromResult(new List<QuestionnaireIdentity>())
                    &&
                    x.GetServerQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) ==
                    Task.FromResult(new List<QuestionnaireIdentity>())
                    &&
                    x.GetInterviewsAsync(Moq.It.IsAny<CancellationToken>()) ==
                    Task.FromResult(new List<InterviewApiView>())
            );

            var interviewerQuestionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(
                x => x.GetCensusQuestionnaireIdentities() == new List<QuestionnaireIdentity>()
                     && x.GetAllQuestionnaireIdentities() == new List<QuestionnaireIdentity>()
            );

            var mockOFInterviewAccessor = new Mock<IInterviewerInterviewAccessor>();

            var viewModel = CreateSynchronizationProcess(principal: principal,
                interviewViewRepository: interviewViewRepository,
                synchronizationService: synchronizationService,
                questionnaireFactory: interviewerQuestionnaireAccessor,
                interviewFactory: mockOFInterviewAccessor.Object
            );

            var progressChangedEventRaised = new ManualResetEvent(false);
            var progressInfo = new Progress<SyncProgressInfo>();

            progressInfo.ProgressChanged += (sender, info) =>
            {
                totalDeletedInterviewsCount = info.Statistics.TotalDeletedInterviewsCount;
                progressChangedEventRaised.Set();
            };
            await viewModel.SyncronizeAsync(progressInfo, CancellationToken.None);
            progressChangedEventRaised.WaitOne();

            mockOFInterviewAccessor.Verify(_ => _.RemoveInterview(interviewId), Times.Once);
            Assert.That(totalDeletedInterviewsCount, Is.EqualTo(1));
        }
    }
}
