using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Tests.Abc;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.Services.SynchronizationProcessTests
{
    internal class when_synchronize_and_census_questionnaire_removed_on_server : SynchronizationProcessTestsContext
    {
        private Establish context = () =>
        {
            var principal = Setup.InterviewerPrincipal("name", "pass");

            interviewId = Guid.Parse("11111111111111111111111111111111");

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

            progressInfo = new Progress<SyncProgressInfo>();
            progressInfo.ProgressChanged += (sender, progressInfo) =>
            {
                totalDeletedInterviewsCount = progressInfo.Statistics.TotalDeletedInterviewsCount;
            };

            synchronizationService = Mock.Of<ISynchronizationService>(
                x => x.GetCensusQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<QuestionnaireIdentity>())
                && x.GetServerQuestionnairesAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<QuestionnaireIdentity>())
                && x.GetInterviewsAsync(Moq.It.IsAny<CancellationToken>()) == Task.FromResult(new List<InterviewApiView>())
                );

            interviewerQuestionnaireAccessor = Mock.Of<IInterviewerQuestionnaireAccessor>(
                x => x.GetCensusQuestionnaireIdentities() == new List<QuestionnaireIdentity>()
                && x.GetAllQuestionnaireIdentities() == new List<QuestionnaireIdentity>()
                );

            mockOFInterviewAccessor = new Mock<IInterviewerInterviewAccessor>();

            viewModel = CreateSynchronizationProcess(principal: principal,
                interviewViewRepository: interviewViewRepository,
                synchronizationService: synchronizationService,
                questionnaireFactory: interviewerQuestionnaireAccessor,
                interviewFactory: mockOFInterviewAccessor.Object
                );
        };
        

        Because of = () => viewModel.SyncronizeAsync(progressInfo, CancellationToken.None).WaitAndUnwrapException();

        It should_progress_report_1_deleted_interview = () =>
            totalDeletedInterviewsCount.ShouldEqual(1);

        It should_remove_1_interview_from_local_storage = () =>
            mockOFInterviewAccessor.Verify(_=>_.RemoveInterview(interviewId), Times.Once);

        static SynchronizationProcess viewModel;
        static ISynchronizationService synchronizationService;
        static IInterviewerQuestionnaireAccessor interviewerQuestionnaireAccessor;
        static Mock<IInterviewerInterviewAccessor> mockOFInterviewAccessor;
        static Progress<SyncProgressInfo> progressInfo;
        static int totalDeletedInterviewsCount;
        static Guid interviewId;
    }
}
