using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.InterviewExportedDataEventHandlerTests
{
    [Subject(typeof(InterviewExportedDataDenormalizer))]
    internal class InterviewExportedDataEventHandlerTestContext
    {
        protected const string firstLevelkey = "#";

        protected static InterviewExportedDataDenormalizer CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
            Func<QuestionnaireDocument> templateCreationAction,
            Func<InterviewData> dataCreationAction = null, Action<InterviewDataExportView> returnStoredView = null,
            UserDocument userDocument = null, IReadSideRepositoryWriter<InterviewActionLog> interviewActionLogWriter = null)
        {
            var dataExportService = new Mock<IDataExportService>();

            if (returnStoredView != null)
                dataExportService.Setup(x => x.AddExportedDataByInterview(Moq.It.IsAny<InterviewDataExportView>()))
                    .Callback(returnStoredView);
            return CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(templateCreationAction, dataCreationAction,
                dataExportService.Object, userDocument, interviewActionLogWriter);
        }

        protected static InterviewExportedDataDenormalizer CreateInterviewExportedDataEventHandlerForQuestionnarieCreatedByMethod(
          Func<QuestionnaireDocument> templateCreationAction,
          Func<InterviewData> dataCreationAction = null, IDataExportService dataExportService = null,
          UserDocument userDocument = null, IReadSideRepositoryWriter<InterviewActionLog> interviewActionLogWriter = null)
        {
            var interviewDataStorageMock = new Mock<IReadSideRepositoryWriter<ViewWithSequence<InterviewData>>>();
            var questionnaire = templateCreationAction();

            interviewDataStorageMock.Setup(
                x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(
                    () =>
                    {
                        var createInterview = dataCreationAction ?? CreateInterviewData;
                        var interview = createInterview();
                        interview.QuestionnaireId = questionnaire.PublicKey;
                        return new ViewWithSequence<InterviewData>(interview, 1);
                    });

            var questionnaireExportStructureMock = new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireExportStructure>>();
            var exportViewFactory = new ExportViewFactory(new ReferenceInfoForLinkedQuestionsFactory(),
                new QuestionnaireRosterStructureFactory());
            questionnaireExportStructureMock.Setup(x => x.GetById(Moq.It.IsAny<string>(), Moq.It.IsAny<long>()))
                .Returns(exportViewFactory.CreateQuestionnaireExportStructure(questionnaire, 1));


            var userDocumentWriter = new Mock<IReadSideRepositoryWriter<UserDocument>>();
            if (userDocument != null)
            {
                userDocumentWriter.Setup(x => x.GetById(Moq.It.IsAny<string>())).Returns(userDocument);
            }

            return new InterviewExportedDataDenormalizer(
                interviewDataStorageMock.Object,
                questionnaireExportStructureMock.Object, dataExportService ?? Mock.Of<IDataExportService>(), userDocumentWriter.Object,
                interviewActionLogWriter ?? Mock.Of<IReadSideRepositoryWriter<InterviewActionLog>>());
        }

        protected static InterviewActionExportView CreateInterviewActionExportView(Guid interviewId, InterviewExportedAction action,string userName="test", string role="headquarter")
        {
            return new InterviewActionExportView(interviewId.FormatGuid(), action, userName, DateTime.Now, role);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(Dictionary<string,Guid> variableNameAndQuestionId)
        {
            var questionnaire = new QuestionnaireDocument();

            foreach (var question in variableNameAndQuestionId)
            {
                questionnaire.Children.Add(new NumericQuestion() { StataExportCaption = question.Key, PublicKey = question.Value, QuestionType = QuestionType.Numeric});
            }

            return questionnaire;
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                    }
                }
            };
        }

        protected static InterviewData CreateInterviewData()
        {
            var interviewData = new InterviewData() {InterviewId = Guid.NewGuid()};
            interviewData.Levels.Add("#", new InterviewLevel(new ValueVector<Guid>(), null, new decimal[0]));
            return interviewData;
        }

        protected static InterviewData CreateInterviewWithAnswers(IEnumerable<Guid> questionsWithAnswers)
        {
            var interviewData = CreateInterviewData();
            foreach (var questionsWithAnswer in questionsWithAnswers)
            {
                var question =
                    interviewData.Levels["#"].GetOrCreateQuestion(questionsWithAnswer);
                question.Answer = "some answer";
            }
            return interviewData;
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

        protected static InterviewDataExportLevelView GetLevel(InterviewDataExportView interviewDataExportView, Guid[] levelVector)
        {
            return interviewDataExportView.Levels.FirstOrDefault(l => l.LevelVector.SequenceEqual(levelVector));
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
                    new NumericQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, 1),
                    new NumericIntegerQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, 1),
                    new DateTimeQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, DateTime.Now),
                    new GeoLocationQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, 1,1,1,1, DateTime.Now),
                    new MultipleOptionsLinkedQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, new decimal[0][]),
                    new SingleOptionLinkedQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, new decimal[0]),
                    new TextListQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, new Tuple<decimal, string>[0]),
                    new QRBarcodeQuestionAnswered(Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, "answer")
                };
            }
        }

        protected static void HandleQuestionAnsweredEventsByDenormalizer(InterviewExportedDataDenormalizer denormalizer, Dictionary<Guid, Tuple<QuestionAnswered, InterviewActionLog>> eventsAndInterviewActionLog)
        {
            foreach (var eventAndInterviewActionLog in eventsAndInterviewActionLog)
            {
                var eventType = eventAndInterviewActionLog.Value.Item1.GetType();
                var publishedEventType = typeof(IPublishedEvent<>).MakeGenericType(eventType);
                MethodInfo createPublishableEventByEventInstanceMethod =
                    typeof (InterviewExportedDataEventHandlerTestContext).GetMethod("CreatePublishableEventByEventInstance",
                        BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo genericCreatePublishableEventByEventInstanceMethod =
                    createPublishableEventByEventInstanceMethod.MakeGenericMethod(eventType);
                var publishedEvent = genericCreatePublishableEventByEventInstanceMethod.Invoke(null,
                    new object[] { eventAndInterviewActionLog.Value.Item1, eventAndInterviewActionLog.Key });

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