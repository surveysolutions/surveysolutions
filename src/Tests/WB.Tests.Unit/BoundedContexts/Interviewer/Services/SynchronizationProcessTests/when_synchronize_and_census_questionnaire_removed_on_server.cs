using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;
using It = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    [TestOf(typeof(RemoveObsoleteQuestionnaires))]
    internal class when_synchronize_and_census_questionnaire_removed_on_server
    {
        [Test]
        public async Task should_progress_report_1_deleted_interview()
        {
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

            var synchronizationService = Mock.Of<IInterviewerSynchronizationService>(
                x =>
                    x.GetCensusQuestionnairesAsync(It.IsAny<CancellationToken>()) == Task.FromResult(new List<QuestionnaireIdentity>()) &&
                    x.GetServerQuestionnairesAsync(It.IsAny<CancellationToken>()) == Task.FromResult(new List<QuestionnaireIdentity>()) &&
                    x.GetInterviewsAsync(It.IsAny<CancellationToken>()) == Task.FromResult(new List<InterviewApiView>())
            );

            var interviewerQuestionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(
                x => x.GetCensusQuestionnaireIdentities() == new List<QuestionnaireIdentity>()
                     && x.GetAllQuestionnaireIdentities() == new List<QuestionnaireIdentity>(new[] { removedQuestionnaireIdentity })
            );

            var interviewsRemover = new Mock<IInterviewsRemover>();

            var synchronizationStep = Create.Service.RemoveObsoleteQuestionnaires(
                synchronizationService: synchronizationService,
                interviewsRemover: interviewsRemover.Object,
                questionnairesAccessor: interviewerQuestionnaireAccessor,
                interviewViewRepository: interviewViewRepository
            );

            await synchronizationStep.ExecuteAsync();

            interviewsRemover.Verify(x => x.RemoveInterviews(It.IsAny<SynchronizationStatistics>(), It.IsAny<IProgress<SyncProgressInfo>>(), interviewId));
        }
    }
}
