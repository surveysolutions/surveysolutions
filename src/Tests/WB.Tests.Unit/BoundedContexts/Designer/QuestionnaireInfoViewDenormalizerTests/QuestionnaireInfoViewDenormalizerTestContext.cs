using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Moq.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireInfoViewDenormalizerTests
{
    [Subject(typeof(QuestionnaireInfoViewDenormalizer))]
    internal class QuestionnaireInfoViewDenormalizerTestContext
    {
        protected static QuestionnaireInfoViewDenormalizer CreateDenormalizer(QuestionnaireInfoView view = null)
        {
            var readSideRepositoryWriter = new Mock<IReadSideKeyValueStorage<QuestionnaireInfoView>>();
            readSideRepositoryWriter.Setup(x => x.GetById(It.IsAny<string>())).Returns(view);

            return new QuestionnaireInfoViewDenormalizer(readSideRepositoryWriter.Object);
        }

        protected static QuestionnaireInfoView CreateQuestionnaireInfoView()
        {
            return new QuestionnaireInfoView() {Chapters = new List<ChapterInfoView>()};
        }

        protected static QuestionnaireInfoView CreateQuestionnaireInfoViewWith1Chapter(string chapterId)
        {
            var questionnaireInfoView = CreateQuestionnaireInfoView();
            questionnaireInfoView.Chapters.Add(new ChapterInfoView() {ItemId = chapterId});
            questionnaireInfoView.GroupsCount = 1;

            return questionnaireInfoView;
        }

        protected static QuestionnaireInfoView CreateQuestionnaireInfoViewWith2Chapters(string chapter1Id, string chapter2Id)
        {
            var questionnaireInfoView = CreateQuestionnaireInfoViewWith1Chapter(chapter1Id);
            questionnaireInfoView.Chapters.Add(new ChapterInfoView() { ItemId = chapter2Id });
            questionnaireInfoView.GroupsCount += 1;

            return questionnaireInfoView;
        }


        protected static QuestionnaireInfoView CreateQuestionnaireInfoViewWith1ChapterAnd1Question(string chapterId)
        {
            var questionnaireInfoView = CreateQuestionnaireInfoViewWith1Chapter(chapterId);
            questionnaireInfoView.QuestionsCount = 1;

            return questionnaireInfoView;
        }

        protected static IPublishedEvent<T> CreatePublishableEvent<T>(T payload)
        {
            return CreatePublishableEvent<T>(payload, Guid.Empty);
        }

        protected static IPublishedEvent<T> CreatePublishableEvent<T>(T payload, Guid eventSourceId)
        {
            var publishableEventMock = new Mock<IPublishedEvent<T>>();
            publishableEventMock.Setup(x => x.Payload).Returns(payload);
            publishableEventMock.Setup(x => x.EventSourceId).Returns(eventSourceId);
            return publishableEventMock.Object;
        }
    }
}
