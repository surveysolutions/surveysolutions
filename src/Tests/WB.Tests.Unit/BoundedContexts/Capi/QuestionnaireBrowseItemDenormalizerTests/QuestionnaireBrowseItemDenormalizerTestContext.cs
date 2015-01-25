using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.QuestionnaireBrowseItemDenormalizerTests
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
        {
            IPublishedEvent<T> e = new PublishedEvent<T>(new UncommittedEvent(Guid.NewGuid(),
                questionnaireId,
                1,
                1,
                DateTime.Now,
                evnt)
                );
            return e;
        }
    }
}
