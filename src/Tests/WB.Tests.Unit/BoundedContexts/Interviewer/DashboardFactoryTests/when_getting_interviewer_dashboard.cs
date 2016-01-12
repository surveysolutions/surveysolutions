using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Interviewer.Implementation.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.DashboardFactoryTests
{
    internal class when_getting_interviewer_dashboard: InterviewerDashboardFactoryTestsContext
    {
        Establish context = () =>
        {
            var questionnaireViewRepository = Mock.Of<IAsyncPlainStorage<QuestionnaireView>>(
                x => x.Query(Moq.It.IsAny<Func<IQueryable<QuestionnaireView>, List<QuestionnaireView>>>()) == new List<QuestionnaireView>() &&
                x.GetById(Moq.It.IsAny<string>()) == new QuestionnaireView { Identity = new QuestionnaireIdentity(new Guid(), 1)});

            var mockOfInterviewsAsyncPlainStorage = new Mock<IAsyncPlainStorage<InterviewView>>();
            mockOfInterviewsAsyncPlainStorage.Setup(x => x.Query(Moq.It.IsAny<Func<IQueryable<InterviewView>, List<InterviewView>>>()))
                .Callback<Func<IQueryable<InterviewView>, List<InterviewView>>>((query) => queryByInterviewerId = query)
                .Returns(emulatedStorageItems);

            var interviewViewModelFactory = Mock.Of<IInterviewViewModelFactory>(
                    x => x.GetNew<InterviewDashboardItemViewModel>() == 
                    new InterviewDashboardItemViewModel(null, null, null, null, null, null, null, questionnaireViewRepository, null, null));
            
            interviewerDashboardFactory = CreateInterviewerDashboardFactory(interviewViewRepository: mockOfInterviewsAsyncPlainStorage.Object,
                questionnaireViewRepository: questionnaireViewRepository,
                interviewViewModelFactory: interviewViewModelFactory);
        };

        Because of = () =>
            interviewerDashboardFactory.GetInterviewerDashboard(interviewerId);
        
        It should_started_interviews_be_filered_by_specified_interviewer = () =>
            queryByInterviewerId.Invoke(emulatedStorageItems.AsQueryable()).All(x=>x.ResponsibleId == interviewerId).ShouldBeTrue();

        private static readonly Guid interviewerId = Guid.Parse("11111111111111111111111111111111");

        private static readonly List<InterviewView> emulatedStorageItems = new List<InterviewView>
        {
            new InterviewView
            {
                ResponsibleId = interviewerId,
                Status = InterviewStatus.Completed,
                AnswersOnPrefilledQuestions = new InterviewAnswerOnPrefilledQuestionView[0],
                GpsLocation = new InterviewGpsLocationView()
            },
            new InterviewView
            {
                ResponsibleId = interviewerId,
                Status = InterviewStatus.RejectedBySupervisor,
                AnswersOnPrefilledQuestions = new InterviewAnswerOnPrefilledQuestionView[0],
                GpsLocation = new InterviewGpsLocationView()
            },
            new InterviewView
            {
                ResponsibleId = interviewerId,
                Status = InterviewStatus.InterviewerAssigned,
                AnswersOnPrefilledQuestions = new InterviewAnswerOnPrefilledQuestionView[0],
                GpsLocation = new InterviewGpsLocationView()
            },
            new InterviewView
            {
                ResponsibleId = interviewerId,
                Status = InterviewStatus.Restarted,
                AnswersOnPrefilledQuestions = new InterviewAnswerOnPrefilledQuestionView[0],
                GpsLocation = new InterviewGpsLocationView()
            },
            new InterviewView
            {
                ResponsibleId = interviewerId,
                Status = InterviewStatus.InterviewerAssigned,
                StartedDateTime = DateTime.Now,
                AnswersOnPrefilledQuestions = new InterviewAnswerOnPrefilledQuestionView[0],
                GpsLocation = new InterviewGpsLocationView()
            },

            new InterviewView
            {
                ResponsibleId = Guid.NewGuid(),
                Status = InterviewStatus.Completed,
                AnswersOnPrefilledQuestions = new InterviewAnswerOnPrefilledQuestionView[0],
                GpsLocation = new InterviewGpsLocationView()
            },
            new InterviewView
            {
                ResponsibleId = Guid.NewGuid(),
                Status = InterviewStatus.RejectedBySupervisor,
                AnswersOnPrefilledQuestions = new InterviewAnswerOnPrefilledQuestionView[0],
                GpsLocation = new InterviewGpsLocationView()
            },
            new InterviewView
            {
                ResponsibleId = Guid.NewGuid(),
                Status = InterviewStatus.InterviewerAssigned,
                AnswersOnPrefilledQuestions = new InterviewAnswerOnPrefilledQuestionView[0],
                GpsLocation = new InterviewGpsLocationView()
            },
            new InterviewView
            {
                ResponsibleId = Guid.NewGuid(),
                Status = InterviewStatus.Restarted,
                AnswersOnPrefilledQuestions = new InterviewAnswerOnPrefilledQuestionView[0],
                GpsLocation = new InterviewGpsLocationView()
            },
            new InterviewView
            {
                ResponsibleId = Guid.NewGuid(),
                Status = InterviewStatus.InterviewerAssigned,
                StartedDateTime = DateTime.Now,
                AnswersOnPrefilledQuestions = new InterviewAnswerOnPrefilledQuestionView[0],
                GpsLocation = new InterviewGpsLocationView()
            }
        };
        private static Func<IQueryable<InterviewView>, List<InterviewView>> queryByInterviewerId;
        private static InterviewerDashboardFactory interviewerDashboardFactory;
    }
}
