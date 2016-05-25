using System;

using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    [Subject(typeof(InterviewSynchronizationDenormalizer))]
    internal class InterviewSynchronizationDenormalizerTestsContext
    {
        protected const string CounterId = "InterviewSyncPackageСounter";

        protected static InterviewSynchronizationDenormalizer CreateDenormalizer(
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures = null,
            IReadSideKeyValueStorage<InterviewData> interviews = null,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummarys = null,
            ISerializer serializer = null,
            IMetaInfoBuilder metaBuilder = null,
            IReadSideRepositoryWriter<InterviewSyncPackageMeta> interviewPackageStorageWriter = null,
            IInterviewSynchronizationDtoFactory synchronizationDtoFactory = null)
        {
            var result = new InterviewSynchronizationDenormalizer(
                questionnriePropagationStructures ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(),
                interviews ?? Mock.Of<IReadSideKeyValueStorage<InterviewData>>(),
                interviewSummarys ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                interviewPackageStorageWriter ??
                Mock.Of<IReadSideRepositoryWriter<InterviewSyncPackageMeta>>(),
                Mock.Of<IReadSideRepositoryWriter<InterviewResponsible>>(),
                synchronizationDtoFactory ?? Mock.Of<IInterviewSynchronizationDtoFactory>());

            return result;
        }

        protected static InterviewSynchronizationDto CreateSynchronizationDto(Guid interviewId)
        {
            return new InterviewSynchronizationDto
                   {
                       Id = interviewId
                   };
        }

    }
}