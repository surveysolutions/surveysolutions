using System;
using System.Collections.Generic;
using FluentAssertions;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_AnswerSet_recived : InterviewHistoryDenormalizerTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            interviewHistoryView = CreateInterviewHistoryView(interviewId);
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new[]
            {
                Create.Entity.DateTimeQuestion(questionId, isTimestamp: false)
            });
            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);

            answerEvents = new List<IEvent>();
            answerEvents.Add(new TextQuestionAnswered(userId, questionId, new decimal[]{1,2}, DateTime.Now, "hi"));
            answerEvents.Add(new MultipleOptionsQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, new decimal[] { 1, 2 }));
            answerEvents.Add(new SingleOptionQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, 1));
            answerEvents.Add(new NumericRealQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, 1));
            answerEvents.Add(new NumericIntegerQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, 1));
            answerEvents.Add(new DateTimeQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, new DateTime(1984,4,18)));
            answerEvents.Add(new GeoLocationQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, 1, 2, 3, 4, new DateTimeOffset(new DateTime(1984, 4, 18))));
            answerEvents.Add(new MultipleOptionsLinkedQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now,
                new decimal[][] { new decimal[] { 1, 3, 3 } }));
            answerEvents.Add(new SingleOptionLinkedQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, new decimal[]{1,2}));
            answerEvents.Add(new TextListQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now,
                new Tuple<decimal, string>[] { new Tuple<decimal, string>(1, "2"), new Tuple<decimal, string>(2, "3") }));
            answerEvents.Add(new QRBarcodeQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, "test"));
            answerEvents.Add(new PictureQuestionAnswered(userId, questionId, new decimal[0], DateTime.Now, "my.png"));
            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
                questionnaireStorage: questionnaireStorage);
            BecauseOf();
        }

         private void BecauseOf() => PublishEventsOnOnInterviewExportedDataDenormalizer(answerEvents, interviewHistoryView, interviewExportedDataDenormalizer);

        [NUnit.Framework.Test] public void should_action_of_TextQuestionAnswered_be_AnswerSet () =>
           interviewHistoryView.Records[0].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_TextQuestionAnswered_be_hi () =>
            interviewHistoryView.Records[0].Parameters["answer"].Should().Be("hi");

        [NUnit.Framework.Test] public void should_roster_Vector_on_TextQuestionAnswered_be_1_2 () =>
          interviewHistoryView.Records[0].Parameters["roster"].Should().Be("1,2");

        [NUnit.Framework.Test] public void should_action_of_MultipleOptionsQuestionAnswered_be_AnswerSet () =>
            interviewHistoryView.Records[1].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_MultipleOptionsQuestionAnswered_be_1_and_2 () =>
            interviewHistoryView.Records[1].Parameters["answer"].Should().Be("1, 2");

        [NUnit.Framework.Test] public void should_action_of_SingleOptionQuestionAnswered_be_AnswerSet () =>
            interviewHistoryView.Records[2].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_SingleOptionQuestionAnswered_be_1 () =>
            interviewHistoryView.Records[2].Parameters["answer"].Should().Be("1");

        [NUnit.Framework.Test] public void should_action_of_NumericRealQuestionAnswered_be_AnswerSet () =>
            interviewHistoryView.Records[3].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_NumericRealQuestionAnswered_be_1 () =>
            interviewHistoryView.Records[3].Parameters["answer"].Should().Be("1");

        [NUnit.Framework.Test] public void should_action_of_NumericIntegerQuestionAnswered_be_AnswerSet () =>
            interviewHistoryView.Records[4].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_NumericIntegerQuestionAnswered_be_1 () =>
            interviewHistoryView.Records[4].Parameters["answer"].Should().Be("1");

        [NUnit.Framework.Test] public void should_action_of_DateTimeQuestionAnswered_be_AnswerSet () =>
            interviewHistoryView.Records[5].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_DateTimeQuestionAnswered_be_4_18_1984 () =>
            interviewHistoryView.Records[5].Parameters["answer"].Should().Be("1984-04-18");

        [NUnit.Framework.Test] public void should_action_of_GeoLocationQuestionAnswered_be_AnswerSet () =>
            interviewHistoryView.Records[6].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_GeoLocationQuestionAnswered_be_1_2_3_4 () =>
            interviewHistoryView.Records[6].Parameters["answer"].Should().Be("1,2[3]4");

        [NUnit.Framework.Test] public void should_action_of_MultipleOptionsLinkedQuestionAnswered_be_AnswerSet () =>
         interviewHistoryView.Records[7].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_MultipleOptionsLinkedQuestionAnswered_be_1_3_3 () =>
            interviewHistoryView.Records[7].Parameters["answer"].Should().Be("1, 3, 3");

        [NUnit.Framework.Test] public void should_action_of_SingleOptionLinkedQuestionAnswered_be_AnswerSet () =>
         interviewHistoryView.Records[8].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_SingleOptionLinkedQuestionAnswered_be_1_2 () =>
            interviewHistoryView.Records[8].Parameters["answer"].Should().Be("1, 2");

        [NUnit.Framework.Test] public void should_action_of_TextListQuestionAnswered_be_AnswerSet () =>
            interviewHistoryView.Records[9].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_TextListQuestionAnswered_be_2_3 () =>
            interviewHistoryView.Records[9].Parameters["answer"].Should().Be("2|3");

        [NUnit.Framework.Test] public void should_action_of_QRBarcodeQuestionAnswered_be_AnswerSet () =>
            interviewHistoryView.Records[10].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_QRBarcodeQuestionAnswered_be_test () =>
            interviewHistoryView.Records[10].Parameters["answer"].Should().Be("test");

        [NUnit.Framework.Test] public void should_action_of_PictureQuestionAnswered_be_AnswerSet () =>
          interviewHistoryView.Records[11].Action.Should().Be(InterviewHistoricalAction.AnswerSet);

        [NUnit.Framework.Test] public void should_answer_on_PictureQuestionAnswered_be_my_png () =>
            interviewHistoryView.Records[11].Parameters["answer"].Should().Be("my.png");

        private static InterviewParaDataEventHandler interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static Guid userId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string variableName = "q1";

        private static List<IEvent> answerEvents;
    }
}
