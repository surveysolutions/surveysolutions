using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using Nito.AsyncEx.Synchronous;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Tests.Unit.SharedKernels.SurveyManagement;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardFactoryTests
{
    internal class when_getting_interviewer_dashboard: InterviewerDashboardFactoryTestsContext
    {
        [OneTimeSetUp]
        public void context()
        {
            var questionnaireViewRepository = new SqliteInmemoryStorage<QuestionnaireView>();
            var questionnaireIdentity = new QuestionnaireIdentity(new Guid(), 1);
            questionnaireViewRepository.Store(new QuestionnaireView
            {
                Id = questionnaireIdentity.ToString()
            });

            foreach (var emulatedStorageItem in emulatedStorageItems)
            {
                emulatedStorageItem.QuestionnaireId = questionnaireIdentity.ToString();
            }
            
            var interviewsAsyncPlainStorage = new SqliteInmemoryStorage<InterviewView>();
            interviewsAsyncPlainStorage.Store(emulatedStorageItems);

            var interviewViewModelFactory = Mock.Of<IInterviewViewModelFactory>(
                    x => x.GetNew<InterviewDashboardItemViewModel>() == 
                    new InterviewDashboardItemViewModel(null, null, null, null, questionnaireViewRepository, new InMemoryPlainStorage<PrefilledQuestionView>(), null));
            
            interviewerDashboardFactory = CreateInterviewerDashboardFactory(interviewViewRepository: interviewsAsyncPlainStorage,
                questionnaireViewRepository: questionnaireViewRepository,
                interviewViewModelFactory: interviewViewModelFactory);
        }

        [SetUp]
        public void because_of() =>
            dashboardByInterviewer = interviewerDashboardFactory.GetInterviewerDashboardAsync(interviewerId);

        [Test]
        public void should_interviews_be_filered_by_specified_interviewer() =>
            dashboardByInterviewer.StartedInterviews
                .Union(dashboardByInterviewer.CompletedInterviews)
                .Union(dashboardByInterviewer.NewInterviews)
                .Union(dashboardByInterviewer.RejectedInterviews)
                .Cast<InterviewDashboardItemViewModel>()
                .All(x => x.InterviewId == startedInterviewId ||
                          x.InterviewId == completedInterviewId ||
                          x.InterviewId == rejectedInterviewId ||
                          x.InterviewId == newInterviewId ||
                          x.InterviewId == restartedInterviewId).ShouldBeTrue();

        static readonly Guid completedInterviewId = Guid.Parse("66666666666666666666666666666666");
        static readonly Guid rejectedInterviewId = Guid.Parse("55555555555555555555555555555555");
        static readonly Guid newInterviewId = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid restartedInterviewId = Guid.Parse("33333333333333333333333333333333");
        static readonly Guid startedInterviewId = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid interviewerId = Guid.Parse("11111111111111111111111111111111");

        static readonly List<InterviewView> emulatedStorageItems = new List<InterviewView>
        {
            new InterviewView
            {
                Id = completedInterviewId.FormatGuid(),
                InterviewId = completedInterviewId,
                ResponsibleId = interviewerId,
                Status = InterviewStatus.Completed,
            },
            new InterviewView
            {
                Id = rejectedInterviewId.FormatGuid(),
                InterviewId = rejectedInterviewId,
                ResponsibleId = interviewerId,
                Status = InterviewStatus.RejectedBySupervisor,
            },
            new InterviewView
            {
                Id = newInterviewId.FormatGuid(),
                InterviewId = newInterviewId,
                ResponsibleId = interviewerId,
                Status = InterviewStatus.InterviewerAssigned,
            },
            new InterviewView
            {
                Id = restartedInterviewId.FormatGuid(),
                InterviewId = restartedInterviewId,
                ResponsibleId = interviewerId,
                Status = InterviewStatus.Restarted,
            },
            new InterviewView
            {
                Id = startedInterviewId.FormatGuid(),
                InterviewId = startedInterviewId,
                ResponsibleId = interviewerId,
                Status = InterviewStatus.InterviewerAssigned,
                StartedDateTime = DateTime.Now,
            },

            new InterviewView
            {
                Id = Guid.NewGuid().FormatGuid(),
                ResponsibleId = Guid.NewGuid(),
                Status = InterviewStatus.Completed,
            },
            new InterviewView
            {
                Id = Guid.NewGuid().FormatGuid(),
                ResponsibleId = Guid.NewGuid(),
                Status = InterviewStatus.RejectedBySupervisor,
            },
            new InterviewView
            {
                Id = Guid.NewGuid().FormatGuid(),
                ResponsibleId = Guid.NewGuid(),
                Status = InterviewStatus.InterviewerAssigned,
            },
            new InterviewView
            {
                Id = Guid.NewGuid().FormatGuid(),
                ResponsibleId = Guid.NewGuid(),
                Status = InterviewStatus.Restarted,
            },
            new InterviewView
            {
                Id = Guid.NewGuid().FormatGuid(),
                ResponsibleId = Guid.NewGuid(),
                Status = InterviewStatus.InterviewerAssigned,
                StartedDateTime = DateTime.Now,
            }
        };
        static DashboardInformation dashboardByInterviewer;
        static InterviewerDashboardFactory interviewerDashboardFactory;
    }
}
