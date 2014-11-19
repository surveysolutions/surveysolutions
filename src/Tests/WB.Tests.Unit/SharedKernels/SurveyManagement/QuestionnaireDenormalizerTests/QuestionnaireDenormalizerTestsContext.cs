using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireDenormalizerTests
{
    [Subject(typeof(QuestionnaireDenormalizer))]
    public class QuestionnaireDenormalizerTestsContext
    {
        protected static QuestionnaireDenormalizer CreateDenormalizer(IQueryableReadSideRepositoryWriter<InterviewSummary> interviews = null,
            IQuestionnaireAssemblyFileAccessor assemblyFileAccessor = null)
        {
            return new QuestionnaireDenormalizer(Mock.Of<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>(),
                Mock.Of<ISynchronizationDataStorage>(),
                Mock.Of<IQuestionnaireCacheInitializer>(),
                Mock.Of<IPlainQuestionnaireRepository>(),
                assemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                interviews ?? Mock.Of<IQueryableReadSideRepositoryWriter<InterviewSummary>>());
        }
    }
}