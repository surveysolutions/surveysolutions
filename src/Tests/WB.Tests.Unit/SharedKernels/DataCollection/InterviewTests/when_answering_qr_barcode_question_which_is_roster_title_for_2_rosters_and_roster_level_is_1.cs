using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_qr_barcode_question_which_is_roster_title_for_2_rosters_and_roster_level_is_1 : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true
                        && _.GetQuestionType(questionId) == QuestionType.QRBarcode
                        && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new[] { rosterAId }
                        && _.DoesQuestionSpecifyRosterTitle(questionId) == true
                        && _.GetRostersAffectedByRosterTitleQuestion(questionId) == new[] { rosterAId, rosterBId }
                );

            IQuestionnaireStorage questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.Apply(Create.Event.RosterInstancesAdded(rosterAId, emptyRosterVector, rosterInstanceId));
            interview.Apply(Create.Event.RosterInstancesAdded(rosterBId, emptyRosterVector, rosterInstanceId));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerQRBarcodeQuestion(userId: userId, questionId: questionId, rosterVector: rosterVector, 
                                              answerTime: DateTime.Now, answer: answer);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_TextListQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<QRBarcodeQuestionAnswered>();

        It should_raise_1_RosterRowsTitleChanged_events = () =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        It should_set_2_affected_roster_ids_in_RosterRowsTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.GroupId)).ToArray()
                .ShouldContainOnly(rosterAId, rosterBId);

        It should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.SequenceEqual(emptyRosterVector)));

        It should_set_last_element_of_roster_vector_to_roster_instance_id_in_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.RosterInstanceId == rosterVector.Last()));

        It should_set_title_to_list_of_answers_divided_by_comma_with_space_in_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(x => x.Title))
                .ShouldEachConformTo(title => title == answer);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static decimal rosterInstanceId = (decimal)22.5;
        private static decimal[] emptyRosterVector = new decimal[] { };
        private static decimal[] rosterVector = emptyRosterVector.Concat(new[] { rosterInstanceId }).ToArray();
        private static string answer = "some answer here";
        
    }
}