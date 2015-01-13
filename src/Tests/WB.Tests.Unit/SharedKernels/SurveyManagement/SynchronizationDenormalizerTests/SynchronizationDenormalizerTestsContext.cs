using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    [Subject(typeof(SynchronizationDenormalizer))]
    internal class SynchronizationDenormalizerTestsContext
    {
        protected static SynchronizationDenormalizer CreateDenormalizer(IReadSideRepositoryWriter<ViewWithSequence<InterviewData>> interviews = null,
            ISynchronizationDataStorage synchronizationDataStorage = null, IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter=null)
        {
            var result = new SynchronizationDenormalizer(
                synchronizationDataStorage ?? Mock.Of<ISynchronizationDataStorage>(), 
                Mock.Of<IReadSideRepositoryWriter<UserDocument>>(),
                Mock.Of<IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>>(),
                interviews ?? Mock.Of<IReadSideRepositoryWriter<ViewWithSequence<InterviewData>>>(),
                interviewSummaryWriter??Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                Mock.Of<IQuestionnaireAssemblyFileAccessor>(), 
                Mock.Of<IPlainQuestionnaireRepository>()
                );

            return result;
        }
    }
}