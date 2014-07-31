using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.EventHandler;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireDenormalizerTests
{
    [Subject(typeof(QuestionnaireDenormalizer))]
    internal class QuestionnaireDenormalizerTestContext
    {
        protected static QuestionnaireDenormalizer CreateQuestionnaireDenormalizer(
            IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned> questionnaireDocumentStorage = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null)
        {
            return new QuestionnaireDenormalizer(
                    questionnaireDocumentStorage ?? Mock.Of<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>(),
                    plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>());
        }

        protected static IPublishedEvent<T> CreatePublishedEvent<T>(Guid questionnaireId, T evnt)
        {
            IPublishedEvent<T> e = new PublishedEvent<T>(new UncommittedEvent(Guid.NewGuid(),
                questionnaireId,
                1,
                1,
                DateTime.Now,
                evnt,
                new Version(1, 0))
                );
            return e;
        }
    }
}
