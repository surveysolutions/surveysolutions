using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
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
    internal class when_answer_on_integer_question_increases_roster_size : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterGroupId = Guid.Parse("11111111111111111111111111111111");

            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId),

                Create.Entity.Roster(rosterId: rosterGroupId, rosterSizeQuestionId: questionWhichIncreasesRosterSizeId),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[] { }, DateTime.Now, 1);

        It should_raise_RosterInstancesAdded_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterGroupId && instance.RosterInstanceId == 0));

        It should_not_raise_RosterInstancesRemoved_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>();

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid rosterGroupId;
    }
}
