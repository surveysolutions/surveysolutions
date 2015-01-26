using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireDenormalizerTests
{
    [Subject(typeof(QuestionnaireDenormalizer))]
    public class QuestionnaireDenormalizerTestsContext
    {
        protected static QuestionnaireDenormalizer CreateDenormalizer(IReadSideRepositoryWriter<InterviewSummary> interviews = null,
            IQuestionnaireAssemblyFileAccessor assemblyFileAccessor = null,
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentStorage = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null)
        {
            return new QuestionnaireDenormalizer(
                questionnaireDocumentStorage ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>(),
                Mock.Of<IQuestionnaireCacheInitializer>(),
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