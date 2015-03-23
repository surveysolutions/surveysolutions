using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_and_questionnaire_has_fixed_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            interviewId = Guid.Parse("11111111111111111111111111111111");
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            answersToFeaturedQuestions = new Dictionary<Guid, object>();
            answersTime = new DateTime(2013, 09, 01);
            var fixedRosterTitles = new string[] { "Title 1", "Title 2", "Title 3" };
            fixedRosterId = Guid.Parse("22220000FFFFFFFFFFFFFFFFFFFFFFFF");

            Guid mandatoryQuestionId = Guid.Parse("33330000FFFFFFFFFFFFFFFFFFFF5555"); 

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.GetFixedRosterGroups(null) == new Guid[] { fixedRosterId }
                && _.GetFixedRosterTitles(fixedRosterId) == fixedRosterTitles
                && _.IsRosterGroup(fixedRosterId) == true
                && _.GetRostersFromTopToSpecifiedGroup(fixedRosterId) == new Guid[] { fixedRosterId }
                );

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, Moq.It.IsAny<long>()) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () =>
            new Interview(interviewId, userId, questionnaireId, 1, answersToFeaturedQuestions, answersTime, supervisorId);

        It should_raise_RosterInstancesAdded_event_with_3_instances = () =>
            eventContext.GetEvent<RosterInstancesAdded>().Instances.Count().ShouldEqual(3);

        It should_raise_RosterInstancesTitleChanged_event_with_3_instances = () =>
          eventContext.GetEvent<RosterInstancesAdded>().Instances.Count().ShouldEqual(3);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid interviewId;
        private static Guid userId;
        private static Guid questionnaireId;
        private static Dictionary<Guid, object> answersToFeaturedQuestions;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid fixedRosterId;
    }
}