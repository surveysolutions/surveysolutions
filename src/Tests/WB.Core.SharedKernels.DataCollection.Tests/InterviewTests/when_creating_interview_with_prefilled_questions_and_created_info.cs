using System;
using System.Linq;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_creating_interview_with_no_featured_questions_ : InterviewTestsContext
    {
        private Establish context = () =>
        {
            eventContext = new EventContext();

            interviewId = Guid.Parse("11000000000000000000000000000000");
            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            responsibleSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA00");
            questionnaireVersion = 18;

            interviewStatus = InterviewStatus.Completed;
            featuredQuestionsMeta = null;
            comments = "status chance comment";
            valid = false;

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.Version == questionnaireVersion);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire &&
                    repository.GetHistoricalQuestionnaire(questionnaireId, questionnaireVersion) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);
        };

        private Because of = () =>
            new Interview(interviewId, userId, questionnaireId, questionnaireVersion, DateTime.Now, responsibleSupervisorId, interviewStatus,
                featuredQuestionsMeta, comments, valid);

        private It should_raise_InterviewCreated_event = () =>
            eventContext.ShouldContainEvent<InterviewOnClientCreated>();

        private It should_provide_questionnaire_id_in_InterviewCreated_event = () =>
            eventContext.GetEvent<InterviewOnClientCreated>()
                .QuestionnaireId.ShouldEqual(questionnaireId);

        private It should_provide_questionnaire_version_in_InterviewCreated_event = () =>
            eventContext.GetEvent<InterviewOnClientCreated>()
                .QuestionnaireVersion.ShouldEqual(questionnaireVersion);

        private It should_raise_SynchronizationMetadataApplied_event = () =>
            eventContext.ShouldContainEvent<SynchronizationMetadataApplied>();

        private It should_provide_status_in_SynchronizationMetadataApplied_event = () =>
            eventContext.GetEvent<SynchronizationMetadataApplied>()
                .Status.ShouldEqual(interviewStatus);

        private It should_provide_comments_in_InterviewStatusChanged_event = () =>
            eventContext.GetEvents<InterviewStatusChanged>().Last()
                .Comment.ShouldEqual(comments);

        private It should_raise_InterviewDeclaredInvalid_event = () =>
            eventContext.ShouldContainEvent<InterviewDeclaredInvalid>();

        
        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static long questionnaireVersion;
        private static Guid userId;
        private static Guid responsibleSupervisorId;
        private static Guid interviewId;

        private static bool valid;
        private static string comments;
        private static AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta;
        private static InterviewStatus interviewStatus;
    }
}