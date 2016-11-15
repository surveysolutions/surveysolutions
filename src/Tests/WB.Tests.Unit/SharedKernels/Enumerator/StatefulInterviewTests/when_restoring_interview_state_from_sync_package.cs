using System;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.DataTransferObjects;
using WB.Core.SharedKernels.Enumerator.Events;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Ignore("KP-8159")]
    internal class when_restoring_interview_state_from_sync_package : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.GetAnswerType(integerQuestionId) == AnswerType.Integer
                && _.GetAnswerType(decimalQuestionId) == AnswerType.Decimal
                && _.GetAnswerType(dateTimeQuestionId) == AnswerType.DateTime
                && _.GetAnswerType(multiOptionQuestionId) == AnswerType.OptionCodeArray
                && _.GetAnswerType(linkedMultiOptionQuestionId) == AnswerType.RosterVectorArray
                && _.GetAnswerType(singleOptionQuestionId) == AnswerType.OptionCode
                && _.GetAnswerType(linkedSingleOptionQuestionId) == AnswerType.RosterVector
                && _.GetAnswerType(listQuestionId) == AnswerType.DecimalAndStringArray
                && _.GetAnswerType(textQuestionId) == AnswerType.String
                && _.GetAnswerType(gpsQestionId) == AnswerType.GpsData
                && _.GetAnswerType(multimediaQuestionId) == AnswerType.FileName);

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

            synchronizationDto = Create.Entity.InterviewSynchronizationDto(questionnaireId: questionnaireId, userId: userId, answers: answersDtos);

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
             interview.RestoreInterviewStateFromSyncPackage(userId, synchronizationDto);

        It should_rise_InterviewSynchronized_event = () =>
             eventContext.ShouldContainEvent<InterviewSynchronized>(x => x.InterviewData == synchronizationDto);

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_correct_userId = () =>
             eventContext.ShouldContainEvent<InterviewAnswersFromSyncPackageRestored>(x => x.UserId == userId);

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_integerQuestion = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(integerQuestionId);
            answerDto.Type.ShouldEqual(AnswerType.Integer);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(integerAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_decimalQuestion = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(decimalQuestionId);
            answerDto.Type.ShouldEqual(AnswerType.Decimal);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(decimalAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_dateTimeQuestion = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(dateTimeQuestionId);
            answerDto.Type.ShouldEqual(AnswerType.DateTime);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(dateTimeAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_singleOptionQuestion = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(singleOptionQuestionId);
            answerDto.Type.ShouldEqual(AnswerType.OptionCode);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(singleOptionAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_linkedSingleOptionQuestion = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(linkedSingleOptionQuestionId);
            answerDto.Type.ShouldEqual(AnswerType.RosterVector);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(linkedSingleAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_multiOptionQuestion = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(multiOptionQuestionId);
            answerDto.Type.ShouldEqual(AnswerType.OptionCodeArray);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(multiOptionAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_linkedMultiOptionQuestion = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(linkedMultiOptionQuestionId);
            answerDto.Type.ShouldEqual(AnswerType.RosterVectorArray);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(linkedMultiAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_listQuestion = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(listQuestionId);
            answerDto.Type.ShouldEqual(AnswerType.DecimalAndStringArray);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(listAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_textQuestion = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(textQuestionId);
            answerDto.Type.ShouldEqual(AnswerType.String);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(textAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_gpsQestionId = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(gpsQestionId);
            answerDto.Type.ShouldEqual(AnswerType.GpsData);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(gpsAnswer);
        };

        It should_rise_InterviewAnswersFromSyncPackageRestored_event_with_right_answer_type_for_multimediaQuestionId = () =>
        {
            var answerDto = GetAnswerDtoFromEvent(multimediaQuestionId);
            answerDto.Type.ShouldEqual(AnswerType.FileName);
            answerDto.RosterVector.ShouldEqual(rosterVector);
            answerDto.Answer.ShouldEqual(multimediaAnswer);
        };

        static InterviewAnswerDto GetAnswerDtoFromEvent(Guid questionId)
        {
            return eventContext.GetSingleEvent<InterviewAnswersFromSyncPackageRestored>().Answers.Single(x => x.Id == questionId);
        }

        static readonly object integerAnswer = 1;
        static readonly object decimalAnswer = 6.44455m;
        static readonly object dateTimeAnswer = DateTime.Now;
        static readonly object singleOptionAnswer = 2m;
        static readonly object linkedSingleAnswer = new []{ 0m, 2m};
        static readonly object multiOptionAnswer = new[] { 1m };
        static readonly object linkedMultiAnswer = new[] { new[] { 1m }, new[] { 2m } };
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
        private static readonly decimal[] rosterVector = new decimal[] { 1m, 0m };
        private static readonly Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
    }
}