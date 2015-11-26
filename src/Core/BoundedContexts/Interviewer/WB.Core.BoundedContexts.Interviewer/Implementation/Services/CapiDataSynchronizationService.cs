using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.BoundedContexts.Interviewer.Views.InterviewMetaInfo;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class CapiDataSynchronizationService : ICapiDataSynchronizationService
    {
        private readonly ILogger logger;
        private readonly ICapiCleanUpService capiCleanUpService;
        private readonly IChangeLogManipulator changelog;
        private readonly ICapiSynchronizationCacheService capiSynchronizationCacheService;
        private readonly ICommandService commandService;
        private readonly ISerializer serializer;
        private readonly IPrincipal principal;
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;

        public CapiDataSynchronizationService(
            IChangeLogManipulator changelog,
            ICommandService commandService,
            ICapiCleanUpService capiCleanUpService,
            ILogger logger,
            ICapiSynchronizationCacheService capiSynchronizationCacheService,
            ISerializer serializer,
            IPrincipal principal,
            IAsyncPlainStorage<InterviewView> interviewViewRepository)
        {
            this.logger = logger;
            this.capiSynchronizationCacheService = capiSynchronizationCacheService;
            this.serializer = serializer;
            this.principal = principal;
            this.interviewViewRepository = interviewViewRepository;
            this.changelog = changelog;
            this.commandService = commandService;
            this.capiCleanUpService = capiCleanUpService;
        }

        public void ProcessDownloadedInterviewPackages(InterviewSyncPackageDto item, string itemType)
        {
            switch (itemType)
            {
                case SyncItemType.Interview:
                    this.UpdateInterview(item);
                    break;
                case SyncItemType.DeleteInterview:
                    this.DeleteInterview(item, this.principal.CurrentUserIdentity.UserId);
                    break;
            }
        }

        public IList<ChangeLogRecordWithContent> GetItemsToPush()
        {
            var records = this.changelog.GetClosedDraftChunksIds(this.principal.CurrentUserIdentity.UserId);
            return records.Select(chunk => new ChangeLogRecordWithContent(chunk.RecordId, chunk.EventSourceId, this.changelog.GetDraftRecordContent(chunk.RecordId))).ToList();
        }

        private void DeleteInterview(InterviewSyncPackageDto item, Guid synchronizedUserId)
        {
            var interviewId = Guid.Parse(item.Content);

            try
            {
                var interviewMetaInfo = this.interviewViewRepository.GetById(item.Content);
                if (interviewMetaInfo != null && interviewMetaInfo.ResponsibleId == synchronizedUserId)
                {
                    this.capiCleanUpService.DeleteInterview(interviewId);
                }
            }
            catch (Exception ex)
            {
                #warning replace catch with propper handler of absent questionnaries
                this.logger.Error("Error on item deletion " + interviewId, ex);
            }
        }

        private void UpdateInterview(InterviewSyncPackageDto item)
        {
            var metaInfo = this.serializer.Deserialize<InterviewMetaInfo>(item.MetaInfo);
            try
            {
                this.capiCleanUpService.DeleteInterview(metaInfo.PublicKey);

                bool createdOnClient = metaInfo.CreatedOnClient.GetValueOrDefault();

                var featuredQuestionsMeta = metaInfo
                    .FeaturedQuestionsMeta
                    .Select(q => new AnsweredQuestionSynchronizationDto(q.PublicKey, new decimal[0], q.Value, string.Empty))
                    .ToArray();

                var applySynchronizationMetadata = new CreateInterviewFromSynchronizationMetadata(
                    metaInfo.PublicKey, 
                    metaInfo.ResponsibleId, 
                    metaInfo.TemplateId, 
                    metaInfo.TemplateVersion,
                    (InterviewStatus)metaInfo.Status,
                    featuredQuestionsMeta, 
                    metaInfo.Comments,
                    metaInfo.RejectDateTime,
                    metaInfo.InterviewerAssignedDateTime,
                    true, 
                    createdOnClient);

                this.commandService.Execute(applySynchronizationMetadata);
             
                this.capiSynchronizationCacheService.SaveItem(metaInfo.PublicKey, item.Content);
            }
            catch (Exception ex)
            {
                this.logger.Error(string.Format("Error while meta applying. Sync package: {0}, interview id: {1}", item.PackageId, metaInfo.PublicKey), ex);
                throw;
            }
        }
    }
}