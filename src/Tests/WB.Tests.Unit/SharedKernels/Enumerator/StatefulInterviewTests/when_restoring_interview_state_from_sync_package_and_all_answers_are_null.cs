using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_restoring_interview_state_from_sync_package_and_all_answers_are_null : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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
                CreateAnsweredQuestionSynchronizationDto(integerQuestionId, rosterVector, null),
                CreateAnsweredQuestionSynchronizationDto(decimalQuestionId, rosterVector, null),
                CreateAnsweredQuestionSynchronizationDto(dateTimeQuestionId, rosterVector, null),
                CreateAnsweredQuestionSynchronizationDto(multiOptionQuestionId, rosterVector, null),
                CreateAnsweredQuestionSynchronizationDto(linkedMultiOptionQuestionId , rosterVector, null),
                CreateAnsweredQuestionSynchronizationDto(singleOptionQuestionId, rosterVector, null),
                CreateAnsweredQuestionSynchronizationDto(linkedSingleOptionQuestionId, rosterVector, null),
                CreateAnsweredQuestionSynchronizationDto(listQuestionId, rosterVector, null),
                CreateAnsweredQuestionSynchronizationDto(textQuestionId, rosterVector, null),
                CreateAnsweredQuestionSynchronizationDto(gpsQestionId, rosterVector, null),
                CreateAnsweredQuestionSynchronizationDto(multimediaQuestionId, rosterVector, null),
            };

            synchronizationDto = Create.Entity.InterviewSynchronizationDto(questionnaireId: questionnaireId,
                userId: userId, answers: answersDtos);

            eventContext = new EventContext();
            BecauseOf();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private void BecauseOf() => interview.Synchronize(Create.Command.Synchronize(userId, synchronizationDto));

        [NUnit.Framework.Test] public void should_rise_InterviewSynchronized_event () =>
             eventContext.ShouldContainEvent<InterviewSynchronized>(x => x.InterviewData == synchronizationDto);

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_integerQuestion () 
        {
            interview.GetIntegerQuestion(Identity.Create(integerQuestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_decimalQuestion () 
        {
            interview.GetDoubleQuestion(Identity.Create(decimalQuestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_dateTimeQuestion () 
        {
            interview.GetDateTimeQuestion(Identity.Create(dateTimeQuestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_singleOptionQuestion () 
        {
            interview.GetSingleOptionQuestion(Identity.Create(singleOptionQuestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_linkedSingleOptionQuestion () 
        {
            interview.GetLinkedSingleOptionQuestion(Identity.Create(linkedSingleOptionQuestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_multiOptionQuestion () 
        {
            interview.GetMultiOptionQuestion(Identity.Create(multiOptionQuestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_linkedMultiOptionQuestion () 
        {
            interview.GetLinkedMultiOptionQuestion(Identity.Create(linkedMultiOptionQuestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_listQuestion () 
        {
            interview.GetTextListQuestion(Identity.Create(listQuestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_textQuestion () 
        {
            interview.GetTextQuestion(Identity.Create(textQuestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_gpsQestionId () 
        {
            interview.GetGpsQuestion(Identity.Create(gpsQestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }

        [NUnit.Framework.Test] public void should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_multimediaQuestionId () 
        {
            interview.GetMultimediaQuestion(Identity.Create(multimediaQuestionId, rosterVector))
                .IsAnswered().Should().BeFalse();
        }
        
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
