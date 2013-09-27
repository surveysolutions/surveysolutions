using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_creating_interview_and_questionnaire_has_question_marked_as_mandatory_inside_initially_disabled_groupInterviewTestsContext : InterviewTestsContext
    {
        private Establish context = () =>
        {
            interviewId = Guid.Parse("11111111111111111111111111111111");
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            mandatoryQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            groupId = Guid.Parse("22220000FFFFFFFFFFFFFFFFFFFFFFFF");
            answersToFeaturedQuestions = new Dictionary<Guid, object>();
            answersTime = new DateTime(2013, 09, 01);

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.GetAllMandatoryQuestions() == new Guid[] { mandatoryQuestionId }
                   &&_.GetAllGroupsWithNotEmptyCustomEnablementConditions() == new[] { groupId }
                   && _.GetAllParentGroupsForQuestion(mandatoryQuestionId) == new[] { groupId }
                );

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            eventContext = new EventContext();
        };

        private Because of = () =>
            new Interview(interviewId, userId, questionnaireId, answersToFeaturedQuestions, answersTime, supervisorId);

        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private It should_not_raise_AnswerDeclaredValid_event = () =>
            eventContext.ShouldNotContainEvent<AnswerDeclaredValid>(@event
                => @event.QuestionId == mandatoryQuestionId);

        private It should_not_raise_AnswerDeclaredInvalid_event = () =>
            eventContext.ShouldNotContainEvent<AnswerDeclaredInvalid>(@event
                => @event.QuestionId == mandatoryQuestionId);

        private static EventContext eventContext;
        private static Guid interviewId;
        private static Guid userId;
        private static Guid questionnaireId;
        private static Dictionary<Guid, object> answersToFeaturedQuestions;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid mandatoryQuestionId;
        private static Guid groupId;
    }
}
