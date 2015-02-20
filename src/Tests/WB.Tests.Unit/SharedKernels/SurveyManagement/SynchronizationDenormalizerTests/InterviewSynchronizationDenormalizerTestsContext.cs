using System;

using Machine.Specifications;
using Moq;

using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.Synchronization.MetaInfo;
using WB.Core.Synchronization.SyncStorage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SynchronizationDenormalizerTests
{
    [Subject(typeof(InterviewSynchronizationDenormalizer))]
    internal class InterviewSynchronizationDenormalizerTestsContext
    {
        protected static InterviewSynchronizationDenormalizer CreateDenormalizer(
            IReadSideKeyValueStorage<QuestionnaireRosterStructure> questionnriePropagationStructures = null,
            IReadSideKeyValueStorage<InterviewData> interviews = null,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummarys = null,
            IJsonUtils jsonUtils = null,
            IMetaInfoBuilder metaBuilder = null,
            IOrderableSyncPackageWriter<InterviewSyncPackageMetaInformation> interviewPackageStorageWriter = null,
            IReadSideKeyValueStorage<InterviewSyncPackageContent> interviewSyncPackageContentStorage=null,
            IInterviewSynchronizationDtoFactory synchronizationDtoFactory = null)
        {
            var result = new InterviewSynchronizationDenormalizer(
                questionnriePropagationStructures ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(),
                interviews ?? Mock.Of<IReadSideKeyValueStorage<InterviewData>>(),
                interviewSummarys ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                jsonUtils ?? Mock.Of<IJsonUtils>(),
                metaBuilder ?? Mock.Of<IMetaInfoBuilder>(),
                interviewPackageStorageWriter ??
                Mock.Of<IOrderableSyncPackageWriter<InterviewSyncPackageMetaInformation>>(),
                Mock.Of<IReadSideRepositoryWriter<InterviewResponsible>>(),
                interviewSyncPackageContentStorage ?? Mock.Of<IReadSideKeyValueStorage<InterviewSyncPackageContent>>(),
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