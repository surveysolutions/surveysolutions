using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_two_nested_rosters_linked_to_the_same_list_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            questionId = Guid.Parse("11111111111111111111111111111111");
            rosterAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");


            var questionnaire = Create.Entity.QuestionnaireDocument(questionnaireId,
                Create.Entity.Group(children: new IComposite[] 
                {
                    Create.Entity.TextListQuestion(questionId: questionId, variable: "list"),
                    Create.Entity.Roster(rosterId: rosterAId,
                        variable: "ros",
                        rosterSizeQuestionId: questionId,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        children:
                            new List<IComposite>()
                            {
                                Create.Entity.Roster(rosterId: rosterBId,
                                    variable: "nestedList",
                                    rosterSizeQuestionId: questionId,
                                    rosterSizeSourceType: RosterSizeSourceType.Question)
                            })
                }));

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.AnswerTextListQuestion(userId, questionId, Empty.RosterVector, DateTime.Now, new[] { Tuple.Create(1m, "one") });

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() => interview.AnswerTextListQuestion(userId, questionId,Empty.RosterVector, DateTime.Now, new[] {Tuple.Create(1m, "one1")});

        [NUnit.Framework.Test] public void should_raise_roster_title_changed () => 
            eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(x => x.ChangedInstances.Second().RosterInstance.OuterRosterVector.Identical(new []{1m}));

        static Guid userId;
        static Guid questionId;
        static Guid rosterAId;
        static Guid rosterBId;
        static Interview interview;
        static EventContext eventContext;
    }
}
