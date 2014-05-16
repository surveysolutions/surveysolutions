using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answer_numeric_which_triggers_4_rosters_3_rosters_one_inside_other_and_1_separate_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            topRosterId = Guid.Parse("11111111111111111111111111111111");
            nestedRosterId = Guid.Parse("21111111111111111111111111111111");
            nestedNestedRosterId = Guid.Parse("31111111111111111111111111111111");
            separateRosterId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion("roster size question")
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric,
                    IsInteger = true,
                    MaxValue = 4
                },
                new Group("top level roster")
                {
                    PublicKey = topRosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group("nested roster")
                        {
                            PublicKey = nestedRosterId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.Question,
                            RosterSizeQuestionId = rosterSizeQuestionId,
                            Children = new List<IComposite>
                            {
                                new Group("nested nested roster")
                                {
                                    PublicKey = nestedNestedRosterId,
                                    IsRoster = true,
                                    RosterSizeSource = RosterSizeSourceType.Question,
                                    RosterSizeQuestionId = rosterSizeQuestionId,
                                }
                            }
                        }
                    }
                },
                new Group("separate roster")
                {
                    PublicKey = separateRosterId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            eventContext = new EventContext();
            interview = CreateInterview(questionnaireId: questionnaireId);
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
            interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, 2);

        It should_produce_one_event_roster_instance_added = () =>
            eventContext.GetEvents<RosterInstancesAdded>().Count().ShouldEqual(1);

        It should_put_16_instances_to_RosterInstancesAdded_event = () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event => @event.Instances.Length == 16);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid topRosterId;
        private static Guid nestedRosterId;
        private static Guid nestedNestedRosterId;
        private static Guid separateRosterId;
        private static Guid rosterSizeQuestionId;
        private static Guid questionnaireId;
    }
}
