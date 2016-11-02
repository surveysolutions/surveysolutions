using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_integer_question_decreasing_roster_size_of_roster_with_nested_roster : InterviewTestsContext
    {
        private Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            nestedRosterGroupId = Guid.Parse("11111111111111111111111111111111");
            parentRosterGroupId = Guid.Parse("21111111111111111111111111111111");

            numericQuestionId = Guid.Parse("22222222222222222222222222222222");
            nestedTextListQuestionId = Guid.Parse("32222222222222222222222222222222");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
               new NumericQuestion("roster size question")
               {
                   PublicKey = numericQuestionId,
                   QuestionType = QuestionType.Numeric,
                   IsInteger = true
               },
               new Group("top level roster")
               {
                   PublicKey = parentRosterGroupId,
                   IsRoster = true,
                   RosterSizeSource = RosterSizeSourceType.Question,
                   RosterSizeQuestionId = numericQuestionId,
                   Children = new List<IComposite>
                   {
                        new TextListQuestion("nested roster size question")
                        {
                            PublicKey = nestedTextListQuestionId,
                            QuestionType = QuestionType.TextList
                        },
                        new Group("nested roster")
                        {
                            PublicKey = nestedRosterGroupId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.Question,
                            RosterSizeQuestionId = nestedTextListQuestionId
                        }
                   }
               });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, new PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.Apply(new NumericIntegerQuestionAnswered(userId, numericQuestionId, new decimal[] { }, DateTime.Now, 2));
            interview.Apply(Create.Event.RosterInstancesAdded(parentRosterGroupId, emptyRosterVector, 0, sortIndex: null));
            interview.Apply(Create.Event.RosterInstancesAdded(parentRosterGroupId, emptyRosterVector, 1, sortIndex: null));

            interview.Apply(new TextListQuestionAnswered(userId, nestedTextListQuestionId, new decimal[] { 1 }, DateTime.Now, previousAnswer));
            interview.Apply(Create.Event.RosterInstancesAdded(nestedRosterGroupId, new decimal[] { 1 }, 1, sortIndex: null));
            interview.Apply(Create.Event.RosterInstancesAdded(nestedRosterGroupId, new decimal[] { 1 }, 2, sortIndex: null));

            eventContext = new EventContext();
        };

        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private Because of = () =>
            interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, new decimal[0], DateTime.Now, 1);


        It should_raise_TextListQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>();

        It should_raise_RosterInstancesRemoved_event_with_2_instances = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count().ShouldEqual(3);

        It should_raise_RosterInstancesRemoved_event_with_2_instances_where_GroupId_equals_to_rosterAId = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.GroupId == parentRosterGroupId).ShouldEqual(1);

        It should_raise_RosterInstancesRemoved_event_with_2_instances_where_GroupId_equals_to_rosterBId = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.GroupId == nestedRosterGroupId).ShouldEqual(2);

        It should_raise_RosterInstancesRemoved_event_with_2_instances_where_roster_instance_id_equals_to_1 = () =>
            eventContext.GetEvent<RosterInstancesRemoved>().Instances.Count(instance => instance.RosterInstanceId == 1).ShouldEqual(2);


        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid numericQuestionId;
        private static Guid nestedTextListQuestionId;
        private static Guid nestedRosterGroupId;
        private static decimal[] emptyRosterVector = new decimal[] { };
        private static Guid parentRosterGroupId;

        private static Tuple<decimal, string>[] previousAnswer = new[]
            {
                new Tuple<decimal, string>(1, "Answer 1"),
                new Tuple<decimal, string>(2, "Answer 2")
            };
    }
}