using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using It = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [NUnit.Framework.TestOf(typeof(InterviewerSynchronizationProcess))]
    internal class when_synchronize_and_census_questionnaire_removed_on_server
    {
        [Test]
        public async Task should_progress_report_1_deleted_interview()
        {
            var principal = Setup.InterviewerPrincipal("name", "pass");

            var interviewId = Guid.NewGuid();
            int totalDeletedInterviewsCount = 0;

            var removedQuestionnaireIdentity = new QuestionnaireIdentity(
                Guid.Parse("11111111111111111111111111111111"), 1);

            var interviewViewRepository = new SqliteInmemoryStorage<InterviewView>();
            interviewViewRepository.Store(new List<InterviewView>
            {
                new InterviewView
                {
                    Id = interviewId.ToString(),
                    InterviewId = interviewId,
                    CanBeDeleted = true,
                    QuestionnaireId = removedQuestionnaireIdentity.ToString()
                }
            });

            var synchronizationService = Mock.Of<ISynchronizationService>(
                x =>
                    x.GetCensusQuestionnairesAsync(It.IsAny<CancellationToken>()) == Task.FromResult(new List<QuestionnaireIdentity>()) &&
                    x.GetServerQuestionnairesAsync(It.IsAny<CancellationToken>()) == Task.FromResult(new List<QuestionnaireIdentity>()) &&
                    x.GetInterviewsAsync(It.IsAny<CancellationToken>()) == Task.FromResult(new List<InterviewApiView>())
            );

            var interviewerQuestionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(
                x => x.GetCensusQuestionnaireIdentities() == new List<QuestionnaireIdentity>()
                     && x.GetAllQuestionnaireIdentities() == new List<QuestionnaireIdentity>(new[] { removedQuestionnaireIdentity })
            );

            var interviewerSyncService = Mock.Of<IInterviewerSynchronizationService>(s => 
                s.GetInterviewerAsync(It.IsAny<RestCredentials>(), It.IsAny<CancellationToken>())
                == Task.FromResult(new InterviewerApiView()));

            var interviewerStorage = Mock.Of<IPlainStorage<InterviewerIdentity>>(ii => ii.FirstOrDefault() == new InterviewerIdentity());

            var mockOfInterviewAccessor = new Mock<IInterviewerInterviewAccessor>();

            var viewModel = Create.Service.SynchronizationProcess(principal: principal,
                interviewViewRepository: interviewViewRepository,
                synchronizationService: synchronizationService,
                interviewerSynchronizationService: interviewerSyncService,
                questionnaireFactory: interviewerQuestionnaireAccessor,
                interviewersPlainStorage: interviewerStorage,
                interviewFactory: mockOfInterviewAccessor.Object
            );

            var progressInfo = new Mock<IProgress<SyncProgressInfo>>();
            progressInfo.Setup(pi => pi.Report(It.IsAny<SyncProgressInfo>()))
                .Callback<SyncProgressInfo>(spi => totalDeletedInterviewsCount = spi.Statistics.TotalDeletedInterviewsCount);

            await viewModel.SynchronizeAsync(progressInfo.Object, CancellationToken.None);

            mockOfInterviewAccessor.Verify(_ => _.RemoveInterview(interviewId), Times.Once);
            Assert.That(totalDeletedInterviewsCount, Is.EqualTo(1));
        }
    }
}
