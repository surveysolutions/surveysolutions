using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_restoring_interview_state_from_sync_package : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var fixedRosterIdentity = Identity.Create(Guid.Parse("11111111111111111111111111111111"),
                Create.Entity.RosterVector(1));
            var fixedNestedRosterIdentity = Identity.Create(Guid.Parse("22222222222222222222222222222222"), rosterVector);
            var fixedNestedNestedRosterIdentity = Identity.Create(Guid.Parse("33333333333333333333333333333333"),
                Create.Entity.RosterVector(1, 0, 3));

            IQuestionnaireStorage questionnaireRepository =
                Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                    questionnaire: Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                    {
                        Create.Entity.FixedRoster(fixedRosterIdentity.Id, fixedTitles: new[] {new FixedRosterTitle(1, "fixed")},
                            children: new[]
                            {
                                Create.Entity.FixedRoster(fixedNestedRosterIdentity.Id, fixedTitles: new[] {new FixedRosterTitle(0, "fixed 2")},
                                    children: new IComposite[]
                                    {
                                        Create.Entity.NumericIntegerQuestion(integerQuestionId),
                                        Create.Entity.NumericRealQuestion(decimalQuestionId),
                                        Create.Entity.DateTimeQuestion(dateTimeQuestionId),
                                        Create.Entity.MultyOptionsQuestion(multiOptionQuestionId),
                                        Create.Entity.TextListQuestion(listQuestionId),
                                        Create.Entity.TextQuestion(textQuestionId),
                                        Create.Entity.GpsCoordinateQuestion(gpsQestionId),
                                        Create.Entity.MultimediaQuestion(multimediaQuestionId),
                                        Create.Entity.SingleOptionQuestion(singleOptionQuestionId),
                                        Create.Entity.MultyOptionsQuestion(linkedMultiOptionQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId),
                                        Create.Entity.SingleOptionQuestion(linkedSingleOptionQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId),
                                        Create.Entity.FixedRoster(fixedNestedNestedRosterIdentity.Id, fixedTitles: new[] {new FixedRosterTitle(3, "fixed 3")},
                                            children: new[]
                                            {
                                                Create.Entity.TextQuestion(sourceOfLinkedQuestionId)
                                            })
                                    })
                            })
                    }));

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            var answersDtos = new[]
            {
                CreateAnsweredQuestionSynchronizationDto(integerQuestionId, rosterVector, integerAnswer),
                CreateAnsweredQuestionSynchronizationDto(decimalQuestionId, rosterVector, decimalAnswer),
                CreateAnsweredQuestionSynchronizationDto(dateTimeQuestionId, rosterVector, dateTimeAnswer),
                CreateAnsweredQuestionSynchronizationDto(multiOptionQuestionId, rosterVector, multiOptionAnswer),
                CreateAnsweredQuestionSynchronizationDto(linkedMultiOptionQuestionId , rosterVector, linkedMultiAnswer),
                CreateAnsweredQuestionSynchronizationDto(singleOptionQuestionId, rosterVector, singleOptionAnswer),
                CreateAnsweredQuestionSynchronizationDto(linkedSingleOptionQuestionId, rosterVector, linkedSingleAnswer),
                CreateAnsweredQuestionSynchronizationDto(listQuestionId, rosterVector, listAnswer),
                CreateAnsweredQuestionSynchronizationDto(textQuestionId, rosterVector, textAnswer),
                CreateAnsweredQuestionSynchronizationDto(gpsQestionId, rosterVector, gpsAnswer),
                CreateAnsweredQuestionSynchronizationDto(multimediaQuestionId, rosterVector, multimediaAnswer),
            };

            var rosterInstances = new Dictionary<InterviewItemId, RosterSynchronizationDto[]>
            {
                {
                    Create.Entity.InterviewItemId(fixedRosterIdentity.Id, fixedRosterIdentity.RosterVector),
                    new[] {Create.Entity.RosterSynchronizationDto(fixedRosterIdentity.Id, fixedRosterIdentity.RosterVector.Shrink(), fixedRosterIdentity.RosterVector.Last())}
                },
                {
                    Create.Entity.InterviewItemId(fixedNestedRosterIdentity.Id, fixedNestedRosterIdentity.RosterVector),
                    new[] {Create.Entity.RosterSynchronizationDto(fixedNestedRosterIdentity.Id, fixedNestedRosterIdentity.RosterVector.Shrink(), fixedNestedRosterIdentity.RosterVector.Last())}
                },
                {
                    Create.Entity.InterviewItemId(fixedNestedNestedRosterIdentity.Id, fixedRosterIdentity.RosterVector),
                    new[] {Create.Entity.RosterSynchronizationDto(fixedNestedNestedRosterIdentity.Id, fixedNestedNestedRosterIdentity.RosterVector.Shrink(), fixedNestedNestedRosterIdentity.RosterVector.Last())}
                }
            };
            synchronizationDto = Create.Entity.InterviewSynchronizationDto(questionnaireId: questionnaireId,
                userId: userId, answers: answersDtos, rosterGroupInstances: rosterInstances);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => interview.RestoreInterviewStateFromSyncPackage(userId, synchronizationDto);

        It should_rise_InterviewSynchronized_event = () =>
             eventContext.ShouldContainEvent<InterviewSynchronized>(x => x.InterviewData == synchronizationDto);

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_integerQuestion = () =>
        {
            interview.GetIntegerQuestion(Identity.Create(integerQuestionId, rosterVector))
                .GetAnswer()
                .Value.ShouldEqual(integerAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_decimalQuestion = () =>
        {
            interview.GetDoubleQuestion(Identity.Create(decimalQuestionId, rosterVector))
                .GetAnswer()
                .Value.ShouldEqual(decimalAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_dateTimeQuestion = () =>
        {
            interview.GetDateTimeQuestion(Identity.Create(dateTimeQuestionId, rosterVector))
                .GetAnswer()
                .Value.ShouldEqual(dateTimeAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_singleOptionQuestion = () =>
        {
            interview.GetSingleOptionQuestion(Identity.Create(singleOptionQuestionId, rosterVector))
                .GetAnswer()
                .SelectedValue.ShouldEqual(singleOptionAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_linkedSingleOptionQuestion = () =>
        {
            interview.GetLinkedSingleOptionQuestion(Identity.Create(linkedSingleOptionQuestionId, rosterVector))
                .GetAnswer()
                .SelectedValue.ShouldEqual(linkedSingleAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_multiOptionQuestion = () =>
        {
            interview.GetMultiOptionQuestion(Identity.Create(multiOptionQuestionId, rosterVector))
                .GetAnswer()
                .ToDecimals().ShouldEqual(multiOptionAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_linkedMultiOptionQuestion = () =>
        {
            interview.GetLinkedMultiOptionQuestion(Identity.Create(linkedMultiOptionQuestionId, rosterVector))
                .GetAnswer().CheckedValues.ShouldEqual(linkedMultiAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_listQuestion = () =>
        {
            interview.GetTextListQuestion(Identity.Create(listQuestionId, rosterVector))
                .GetAnswer()
                .ToTupleArray().ShouldEqual(listAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_textQuestion = () =>
        {
            interview.GetTextQuestion(Identity.Create(textQuestionId, rosterVector))
                .GetAnswer()
                .Value.ShouldEqual(textAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_gpsQestionId = () =>
        {
            interview.GetGpsQuestion(Identity.Create(gpsQestionId, rosterVector))
                .GetAnswer()
                .Value.ShouldEqual(gpsAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_multimediaQuestionId = () =>
        {
            interview.GetMultimediaQuestion(Identity.Create(multimediaQuestionId, rosterVector))
                .GetAnswer()
                .FileName.ShouldEqual(multimediaAnswer);
        };

        static readonly object integerAnswer = 1;
        static readonly object decimalAnswer = 6.44455;
        static readonly object dateTimeAnswer = DateTime.Now;
        static readonly int singleOptionAnswer = 2;
        static readonly object linkedSingleAnswer = Create.Entity.RosterVector(0,2);
        static readonly object multiOptionAnswer = new[] { 1m };
        static readonly object linkedMultiAnswer = new [] { Create.Entity.RosterVector(1), Create.Entity.RosterVector(2) };
        static readonly object listAnswer = new []{ new Tuple<decimal, string>(2,"Hello") };
        static readonly object textAnswer = "hello";
        static readonly object gpsAnswer = new GeoPosition(1, 2, 3, 4, DateTime.Now);
        static readonly object multimediaAnswer = "hello.jpeg";
        private static EventContext eventContext;
        private static InterviewSynchronizationDto synchronizationDto;
        private static StatefulInterview interview;
        private static readonly Guid integerQuestionId = Guid.Parse("00000000000000000000000000000001");
        private static readonly Guid decimalQuestionId = Guid.Parse("00000000000000000000000000000002");
        private static readonly Guid dateTimeQuestionId = Guid.Parse("00000000000000000000000000000003");
        private static readonly Guid singleOptionQuestionId = Guid.Parse("00000000000000000000000000000004");
        private static readonly Guid linkedSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000005");
        private static readonly Guid multiOptionQuestionId = Guid.Parse("00000000000000000000000000000006");
        private static readonly Guid linkedMultiOptionQuestionId = Guid.Parse("00000000000000000000000000000007");
        private static readonly Guid listQuestionId = Guid.Parse("00000000000000000000000000000008");
        private static readonly Guid textQuestionId = Guid.Parse("00000000000000000000000000000009");
        private static readonly Guid gpsQestionId = Guid.Parse("00000000000000000000000000000010");
        private static readonly Guid multimediaQuestionId = Guid.Parse("00000000000000000000000000000011");
        private static readonly Guid sourceOfLinkedQuestionId = Guid.Parse("00000000000000000000000000000012");
        private static readonly decimal[] rosterVector = new decimal[] { 1m, 0m };
        private static readonly Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
    }
}