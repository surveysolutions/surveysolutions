using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Interviewer.EventHandler;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Interviewer.QuestionnaireBrowseItemDenormalizerTests
{
    [Subject(typeof(QuestionnaireBrowseItemDenormalizer))]
    internal class QuestionnaireBrowseItemDenormalizerTestContext
    {
        protected static QuestionnaireBrowseItemDenormalizer CreateQuestionnaireBrowseItemDenormalizer(
            IReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaireBrowseItemStorage=null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository=null)
        {
            return
                new QuestionnaireBrowseItemDenormalizer(
                    questionnaireBrowseItemStorage ?? Mock.Of<IReadSideRepositoryWriter<QuestionnaireBrowseItem>>(),
                    plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>());
        }

        protected static IPublishedEvent<T> CreatePublishedEvent<T>(Guid questionnaireId, T evnt)
            where T: ILiteEvent
        {
            return new PublishedEvent<T>(Create.PublishableEvent(eventSourceId: questionnaireId, payload: evnt));
        }
    }
}
