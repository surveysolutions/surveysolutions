using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    internal class InterviewHistoryDenormalizerTests : InterviewHistoryDenormalizerTestContext
    {
        [Test]
        public void when_AnswerSet_recived_with_dateTimeOffset()
        {

            Guid interviewId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid questionId = Guid.Parse("11111111111111111111111111111111");
            string variableName = "q1";


            var interviewHistoryView = CreateInterviewHistoryView(interviewId);
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new[]
            {
                Create.Entity.TextQuestion(questionId)
            });
            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);

            var dateTimeOffset = DateTimeOffset.Now;

            var answerEvents = new List<IEvent>();
            answerEvents.Add(new TextQuestionAnswered(userId, questionId, new decimal[] {1, 2}, dateTimeOffset, "hi"));

            var interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
                questionnaireStorage: questionnaireStorage);

            //act
            PublishEventsOnOnInterviewExportedDataDenormalizer(answerEvents, interviewHistoryView,
                interviewExportedDataDenormalizer);


            Assert.AreEqual(interviewHistoryView.Records[0].Parameters["answer"], "hi");
            Assert.AreEqual(interviewHistoryView.Records[0].Timestamp, dateTimeOffset.LocalDateTime);
            Assert.AreEqual(interviewHistoryView.Records[0].Offset, dateTimeOffset.Offset);

        }

        [TestCase(typeof(QuestionsDisabled))]
        [TestCase(typeof(QuestionsEnabled))]
        [TestCase(typeof(GroupsDisabled))]
        [TestCase(typeof(GroupsEnabled))]
        public void should_not_impliment_handlers_for_events(Type eventType)
        {
            var interfaceForEventType = typeof(IUpdateHandler<,>).MakeGenericType(typeof(InterviewHistoryView), eventType);
            var isImplimentedInterface = interfaceForEventType.IsAssignableFrom(typeof(InterviewParaDataEventHandler));

            Assert.False(isImplimentedInterface);
        }

    }
}
