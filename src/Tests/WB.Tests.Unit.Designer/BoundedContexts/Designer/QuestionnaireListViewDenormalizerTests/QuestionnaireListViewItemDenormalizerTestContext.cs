using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using It = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireListViewDenormalizerTests
{
    [Subject(typeof(QuestionnaireListViewItemDenormalizer))]
    internal class QuestionnaireListViewItemDenormalizerTestContext
    {
        protected static QuestionnaireListViewItemDenormalizer CreateQuestionnaireListViewItemDenormalizer(
            IReadSideRepositoryWriter<QuestionnaireListViewItem> questionnaireListViewItemWriter = null,
            IReadSideRepositoryWriter<AccountDocument> accountDocumentWriter = null)
        {
            return
                new QuestionnaireListViewItemDenormalizer(
                    questionnaireListViewItemWriter ?? Mock.Of<IReadSideRepositoryWriter<QuestionnaireListViewItem>>(),
                    accountDocumentWriter ??
                        Mock.Of<IReadSideRepositoryWriter<AccountDocument>>(
                            _ => _.GetById(It.IsAny<string>()) == new AccountDocument() { UserName = "nastya" }));
        }

        protected static CommittedEvent CreateCommittedEvent(IEvent payload, Guid eventSourceId, DateTime eventTimeStamp)
        {
            return new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), eventSourceId, 1, eventTimeStamp, 0, payload);
        }

        protected static IPublishedEvent<T> CreatePublishedEvent<T>(Guid eventSourceId,T payload, DateTime? timeStamp=null)
            where T: IEvent
        {
            var mock = new Mock<IPublishedEvent<T>>();
            mock.Setup(x => x.Payload).Returns(payload);
            mock.Setup(x => x.EventSourceId).Returns(eventSourceId);
            if (timeStamp.HasValue)
            {
                mock.Setup(x => x.EventTimeStamp).Returns(timeStamp.Value);
            }
            return mock.Object;
        }


    }
}
