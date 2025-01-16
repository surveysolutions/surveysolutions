using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
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


            ClassicAssert.AreEqual(interviewHistoryView.Records[0].Parameters["answer"], "hi");
            ClassicAssert.AreEqual(interviewHistoryView.Records[0].Timestamp, dateTimeOffset.UtcDateTime);
            ClassicAssert.AreEqual(interviewHistoryView.Records[0].Offset, dateTimeOffset.Offset);

        }

        [TestCase(typeof(QuestionsDisabled))]
        [TestCase(typeof(QuestionsEnabled))]
        [TestCase(typeof(GroupsDisabled))]
        [TestCase(typeof(GroupsEnabled))]
        public void should_not_impliment_handlers_for_events(Type eventType)
        {
            var interfaceForEventType = typeof(IUpdateHandler<,>).MakeGenericType(typeof(InterviewHistoryView), eventType);
            var isImplimentedInterface = interfaceForEventType.IsAssignableFrom(typeof(InterviewParaDataEventHandler));

            ClassicAssert.False(isImplimentedInterface);
        }

        [Test]
        public void when_answer_removed_Should_subtitute_variable_name_instead_of_id()
        {
            Guid interviewId = Id.gA;
            Guid questionId = Id.g1;
            string variableName = "q1";

            var interviewHistoryView = CreateInterviewHistoryView(interviewId);
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new[]
            {
                Create.Entity.TextQuestion(questionId, variable: variableName)
            });

            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);

            var answerEvents = new List<IEvent>();
            answerEvents.Add(Create.Event.AnswersRemoved(Create.Identity(questionId)));

            var interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(
               questionnaire: CreateQuestionnaireExportStructure(questionId, variableName),
               questionnaireStorage: questionnaireStorage);

            //act
            PublishEventsOnOnInterviewExportedDataDenormalizer(answerEvents, interviewHistoryView,
                interviewExportedDataDenormalizer);

            ClassicAssert.AreEqual(interviewHistoryView.Records[0].Parameters["question"], variableName);
        }

        [Test]
        public void should_provide_user_id_when_remove_answer()
        {
            var interviewHistoryView = CreateInterviewHistoryView(Id.g1);
            var questionnaireDocument = Create.Entity.QuestionnaireDocument(children: new[]
            {
                Create.Entity.TextQuestion(Id.g2, variable: "text")
            });

            var questionnaireStorage = Stub<IQuestionnaireStorage>.Returning(questionnaireDocument);

            var answerEvents = new List<IEvent>();
            answerEvents.Add(Create.Event.AnswersRemoved(Id.g10, Create.Identity(Id.g2)));

            UserViewLite user = Mock.Of<UserViewLite>(u => u.UserName == "User"
                                                           && u.Roles == new HashSet<UserRoles>(new[] { UserRoles.Supervisor }));
            var userViewFactory = Mock.Of<IUserViewFactory>(u => u.GetUser(Id.g10) == user);

            var interviewExportedDataDenormalizer = CreateInterviewHistoryDenormalizer(
                questionnaire: CreateQuestionnaireExportStructure(Id.g2, "text"),
                questionnaireStorage: questionnaireStorage,
                userDocumentWriter: userViewFactory);

            //act
            PublishEventsOnOnInterviewExportedDataDenormalizer(answerEvents, interviewHistoryView,
                interviewExportedDataDenormalizer);

            ClassicAssert.AreEqual(interviewHistoryView.Records[0].Parameters["question"], "text");
            ClassicAssert.AreEqual(interviewHistoryView.Records[0].OriginatorName, "User");
            ClassicAssert.AreEqual(interviewHistoryView.Records[0].OriginatorRole, "Supervisor");
        }
    }
}
