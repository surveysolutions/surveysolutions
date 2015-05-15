using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class when_AnswerSet_recived : InterviewHistoryDenormalizerTestContext
    {
        Establish context = () =>
        {
            interviewHistoryView = CreateInterviewHistoryView(interviewId);
            answerEvents = new List<object>();
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
            interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(questionnaire: CreateQuestionnaireExportStructure(questionId, variableName));
        };

         Because of =
            () => PublishEventsOnOnInterviewExportedDataDenormalizer(answerEvents, interviewHistoryView, interviewExportedDataDenormalizer);

        It should_action_of_TextQuestionAnswered_be_AnswerSet = () =>
           interviewHistoryView.Records[0].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_TextQuestionAnswered_be_hi = () =>
            interviewHistoryView.Records[0].Parameters["answer"].ShouldEqual("hi");

        It should_roster_Vector_on_TextQuestionAnswered_be_1_2 = () =>
          interviewHistoryView.Records[0].Parameters["roster"].ShouldEqual("1,2");

        It should_action_of_MultipleOptionsQuestionAnswered_be_AnswerSet = () =>
            interviewHistoryView.Records[1].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_MultipleOptionsQuestionAnswered_be_1_and_2 = () =>
            interviewHistoryView.Records[1].Parameters["answer"].ShouldEqual("1, 2");

        It should_action_of_SingleOptionQuestionAnswered_be_AnswerSet = () =>
            interviewHistoryView.Records[2].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_SingleOptionQuestionAnswered_be_1 = () =>
            interviewHistoryView.Records[2].Parameters["answer"].ShouldEqual("1");

        It should_action_of_NumericRealQuestionAnswered_be_AnswerSet = () =>
            interviewHistoryView.Records[3].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_NumericRealQuestionAnswered_be_1 = () =>
            interviewHistoryView.Records[3].Parameters["answer"].ShouldEqual("1");

        It should_action_of_NumericIntegerQuestionAnswered_be_AnswerSet = () =>
            interviewHistoryView.Records[4].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_NumericIntegerQuestionAnswered_be_1 = () =>
            interviewHistoryView.Records[4].Parameters["answer"].ShouldEqual("1");

        It should_action_of_DateTimeQuestionAnswered_be_AnswerSet = () =>
            interviewHistoryView.Records[5].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_DateTimeQuestionAnswered_be_4_18_1984 = () =>
            interviewHistoryView.Records[5].Parameters["answer"].ShouldEqual("4/18/1984");

        It should_action_of_GeoLocationQuestionAnswered_be_AnswerSet = () =>
            interviewHistoryView.Records[6].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_GeoLocationQuestionAnswered_be_1_2_3_4 = () =>
            interviewHistoryView.Records[6].Parameters["answer"].ShouldEqual("1,2[3]4");

        It should_action_of_MultipleOptionsLinkedQuestionAnswered_be_AnswerSet = () =>
         interviewHistoryView.Records[7].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_MultipleOptionsLinkedQuestionAnswered_be_1_3_3 = () =>
            interviewHistoryView.Records[7].Parameters["answer"].ShouldEqual("1, 3, 3");

        It should_action_of_SingleOptionLinkedQuestionAnswered_be_AnswerSet = () =>
         interviewHistoryView.Records[8].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_SingleOptionLinkedQuestionAnswered_be_1_2 = () =>
            interviewHistoryView.Records[8].Parameters["answer"].ShouldEqual("1, 2");

        It should_action_of_TextListQuestionAnswered_be_AnswerSet = () =>
            interviewHistoryView.Records[9].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_TextListQuestionAnswered_be_2_3 = () =>
            interviewHistoryView.Records[9].Parameters["answer"].ShouldEqual("2|3");

        It should_action_of_QRBarcodeQuestionAnswered_be_AnswerSet = () =>
            interviewHistoryView.Records[10].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_QRBarcodeQuestionAnswered_be_test = () =>
            interviewHistoryView.Records[10].Parameters["answer"].ShouldEqual("test");

        It should_action_of_PictureQuestionAnswered_be_AnswerSet = () =>
          interviewHistoryView.Records[11].Action.ShouldEqual(InterviewHistoricalAction.AnswerSet);

        It should_answer_on_PictureQuestionAnswered_be_my_png = () =>
            interviewHistoryView.Records[11].Parameters["answer"].ShouldEqual("my.png");

        private static InterviewHistoryDenormalizer interviewExportedDataDenormalizer;
        private static Guid interviewId = Guid.NewGuid();
        private static Guid userId = Guid.NewGuid();
        private static InterviewHistoryView interviewHistoryView;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static string variableName = "q1";

        private static List<object> answerEvents;
    }
}
