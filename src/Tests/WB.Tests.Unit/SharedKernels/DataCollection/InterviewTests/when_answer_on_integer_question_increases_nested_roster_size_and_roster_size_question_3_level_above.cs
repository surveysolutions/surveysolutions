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
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_on_integer_question_increases_nested_roster_size_and_roster_size_question_3_level_above : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            firstLevelRosterId = Guid.Parse("21111111111111111111111111111111");

            secondLevelRosterId = Guid.Parse("31111111111111111111111111111111");
            thirdLevelRosterId = Guid.Parse("11111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: numericQuestionId),
                Create.Entity.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId),

                Create.Entity.Roster(rosterId: firstLevelRosterId, rosterSizeQuestionId: numericQuestionId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                {
                    Create.Entity.Roster(rosterId: secondLevelRosterId, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.Entity.Roster(rosterId: thirdLevelRosterId, rosterSizeQuestionId: questionWhichIncreasesRosterSizeId),
                    }),
                }),
            }));


            var questionnaire2 = Mock.Of<IQuestionnaire>(_

                                                        => _.HasQuestion(questionWhichIncreasesRosterSizeId) == true
                                                        && _.GetQuestionType(questionWhichIncreasesRosterSizeId) == QuestionType.Numeric
                                                        && _.IsQuestionInteger(questionWhichIncreasesRosterSizeId) == true
                                                        && _.GetRosterGroupsByRosterSizeQuestion(questionWhichIncreasesRosterSizeId) == new[] { thirdLevelRosterId }

                                                        && _.HasGroup(thirdLevelRosterId) == true
                                                        && _.HasGroup(secondLevelRosterId) == true
                                                        && _.HasGroup(firstLevelRosterId) == true
                                                        && _.GetRosterLevelForGroup(thirdLevelRosterId) == 3
                                                        && _.GetRosterLevelForGroup(secondLevelRosterId) == 2
                                                        && _.GetRosterLevelForGroup(firstLevelRosterId) == 1

                                                        //&& _.GetGroupAndUnderlyingGroupsWithNotEmptyCustomEnablementConditions(thirdLevelRosterId) == new[] { firstLevelRosterId,secondLevelRosterId, thirdLevelRosterId }
                                                        && _.GetRostersFromTopToSpecifiedGroup(thirdLevelRosterId) == new[] { firstLevelRosterId, secondLevelRosterId, thirdLevelRosterId }
                                                        && _.GetRostersFromTopToSpecifiedGroup(secondLevelRosterId) == new[] { firstLevelRosterId, secondLevelRosterId }
                                                        && _.GetRostersFromTopToSpecifiedGroup(firstLevelRosterId) == new[] { firstLevelRosterId }
                                                        && _.GetRostersFromTopToSpecifiedQuestion(questionWhichIncreasesRosterSizeId) == new Guid[0]);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.NumericIntegerQuestionAnswered(numericQuestionId, RosterVector.Empty, 1));
            interview.Apply(Create.Event.RosterInstancesAdded(firstLevelRosterId, new decimal[0], 0, null));
            interview.Apply(Create.Event.RosterInstancesAdded(secondLevelRosterId, new decimal[] { 0 }, 0, null));
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0], DateTime.Now, 1);

        It should_raise_RosterInstancesAdded_event_for_first_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.Any(instance => instance.GroupId == thirdLevelRosterId && instance.RosterInstanceId == 0 && instance.OuterRosterVector.SequenceEqual(new decimal[]{0,0})));

        It should_not_raise_RosterInstancesRemoved_event = () =>
            eventContext.ShouldNotContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == thirdLevelRosterId));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid thirdLevelRosterId;
        private static Guid secondLevelRosterId;
        private static Guid firstLevelRosterId;
        private static Guid numericQuestionId = Guid.Parse("33333333333333333333333333333333");
    }
}
