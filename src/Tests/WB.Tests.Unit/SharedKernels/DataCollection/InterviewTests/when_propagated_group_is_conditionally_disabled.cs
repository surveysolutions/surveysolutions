using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using it = Moq.It;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [Ignore("KP-4387")]
    internal class when_propagated_group_is_conditionally_disabled : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            propagatedGroupId = Guid.Parse("11111111111111111111111111111111");
           
            questionWhichIsForcesPropagationId = Guid.Parse("22222222222222222222222222222222");

            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.HasQuestion(questionWhichIsForcesPropagationId) == true
                && _.GetQuestionType(questionWhichIsForcesPropagationId) == QuestionType.AutoPropagate
                && _.IsQuestionInteger(questionWhichIsForcesPropagationId) == true);

            var enablementChanges = Create.Entity.EnablementChanges(
                groupsToBeDisabled: new List<Identity> { Create.Entity.Identity(propagatedGroupId, Empty.RosterVector) });

            Setup.SelfCloningInterviewExpressionStateStubWithProviderToMockedServiceLocator(questionnaireId, _
                => _.ProcessEnablementConditions() == enablementChanges);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIsForcesPropagationId, new decimal[] { }, DateTime.Now, 1);

        It should_raise_GroupsDisabled_event_with_GroupId_equal_to_propagatedGroupId_with_disablement_condition = () =>
            eventContext.ShouldContainEvent<GroupsDisabled>(@event
                => @event.Groups.Any(group => group.Id == propagatedGroupId));
       
        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIsForcesPropagationId;
        private static Guid propagatedGroupId;
    }
}
