using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    [Subject(typeof(InterviewExportedDataDenormalizer))]
    internal class InterviewExportedDataEventHandlerTestContext
    {
        protected const string firstLevelkey = "#";

        protected static InterviewExportedDataDenormalizer CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
          IDataExportRepositoryWriter dataExportRepositoryWriter = null,
          IReadSideKeyValueStorage<RecordFirstAnswerMarkerView> recordFirstAnswerMarkerViewStorage = null, UserDocument user = null, InterviewSummary interviewSummary = null)
        {
            return new InterviewExportedDataDenormalizer(dataExportRepositoryWriter ?? Mock.Of<IDataExportRepositoryWriter>(),
                recordFirstAnswerMarkerViewStorage ?? Mock.Of<IReadSideKeyValueStorage<RecordFirstAnswerMarkerView>>(),
                Mock.Of<IReadSideRepositoryWriter<UserDocument>>(_ => _.GetById(It.IsAny<string>()) == user),
                Mock.Of<IReadSideRepositoryReader<InterviewSummary>>(_ => _.GetById(It.IsAny<string>()) == interviewSummary),null);
        }

        protected static InterviewCommentedStatus CreateInterviewCommentedStatus(InterviewStatus status)
        {
            return new InterviewCommentedStatus() { Status = status, InterviewerId = Guid.NewGuid() };
        }

        protected static InterviewActionExportView CreateInterviewActionExportView(Guid interviewId, InterviewExportedAction action,string userName="test", string role="headquarter")
        {
            return new InterviewActionExportView(interviewId.FormatGuid(), action, userName, DateTime.Now, role);
        }

        protected static IPublishedEvent<InterviewApprovedByHQ> CreateInterviewApprovedByHQPublishableEvent(Guid? interviewId=null)
        {
             var eventSourceId = interviewId ?? Guid.NewGuid();
            return CreatePublishableEvent(() => new InterviewApprovedByHQ(eventSourceId, ""), eventSourceId);
        }

        protected static IPublishedEvent<T> CreatePublishableEvent<T>(Func<T> eventCreator, Guid? eventSourceId = null)
        {
            var publishableEventMock = new Mock<IPublishedEvent<T>>();

            publishableEventMock.Setup(x => x.Payload).Returns(eventCreator());
            publishableEventMock.Setup(x => x.EventSourceId).Returns(eventSourceId ?? Guid.NewGuid());
            
            return publishableEventMock.Object;
        }

        protected static IPublishedEvent<T> CreatePublishableEventByEventInstance<T>(T eventInstance, Guid? eventSourceId = null)
        {
            var publishableEventMock = new Mock<IPublishedEvent<T>>();

            publishableEventMock.Setup(x => x.Payload).Returns(eventInstance);
            publishableEventMock.Setup(x => x.EventSourceId).Returns(eventSourceId ?? Guid.NewGuid());

            return publishableEventMock.Object;
        }

        protected static List<QuestionAnswered> ListOfQuestionAnsweredEventsHandledByDenormalizer
        {
            get
            {
                return new List<QuestionAnswered>
                {
                    new TextQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, "answer"),
                    new MultipleOptionsQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, new decimal[0]),
                    new SingleOptionQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, 1),
                    new NumericRealQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, 1),
                    new NumericIntegerQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, 1),
                    new DateTimeQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, DateTime.Now),
                    new GeoLocationQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, 1,1,1,1, DateTime.Now),
                    new MultipleOptionsLinkedQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, new decimal[0][]),
                    new SingleOptionLinkedQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, new decimal[0]),
                    new TextListQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, new Tuple<decimal, string>[0]),
                    new QRBarcodeQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, "answer"),
                    new PictureQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, "answer.png")
                };
            }
        }

        protected static void HandleQuestionAnsweredEventsByDenormalizer(InterviewExportedDataDenormalizer denormalizer, Dictionary<Guid, QuestionAnswered> eventsAndInterviewActionLog)
        {
            foreach (var eventAndInterviewActionLog in eventsAndInterviewActionLog)
            {
                var eventType = eventAndInterviewActionLog.Value.GetType();
                var publishedEventType = typeof(IPublishedEvent<>).MakeGenericType(eventType);
                MethodInfo createPublishableEventByEventInstanceMethod =
                    typeof (InterviewExportedDataEventHandlerTestContext).GetMethod("CreatePublishableEventByEventInstance",
                        BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo genericCreatePublishableEventByEventInstanceMethod =
                    createPublishableEventByEventInstanceMethod.MakeGenericMethod(eventType);
                var publishedEvent = genericCreatePublishableEventByEventInstanceMethod.Invoke(null,
                    new object[] { eventAndInterviewActionLog.Value, eventAndInterviewActionLog.Key });

                MethodInfo methodInfo = denormalizer.GetType().GetMethod("Handle", new[] { publishedEventType });

                methodInfo.Invoke(denormalizer, new[] { publishedEvent });
            }
        }
    }

    public static class ShouldExtensions
    {
        public static void ShouldQuestionHasOneNotEmptyAnswer(this ExportedQuestion[] questions, Guid questionId)
        {
            questions.ShouldContain(q => q.QuestionId == questionId);
            var answers = questions.First(q => q.QuestionId == questionId).Answers;
            answers.ShouldContain(a => !string.IsNullOrEmpty(a));
        }

        public static void ShouldQuestionHasNoAnswers(this ExportedQuestion[] questions, Guid questionId)
        {
            questions.ShouldNotContain(q => q.QuestionId == questionId && q.Answers.Any(a=>!string.IsNullOrEmpty(a)));
        }
    }

}