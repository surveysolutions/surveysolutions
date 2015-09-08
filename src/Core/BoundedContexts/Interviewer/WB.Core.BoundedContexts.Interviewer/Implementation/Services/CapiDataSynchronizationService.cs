using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views.InterviewMetaInfo;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class CapiDataSynchronizationService : ICapiDataSynchronizationService
    {
        private readonly ILogger logger;
        private readonly ICapiCleanUpService capiCleanUpService;
        private readonly IChangeLogManipulator changelog;
        private readonly ICapiSynchronizationCacheService capiSynchronizationCacheService;
        private readonly ICommandService commandService;
        private readonly IJsonUtils jsonUtils;
        private readonly IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo> interviewIntoFactory;
        private readonly IPrincipal principal;

        public CapiDataSynchronizationService(
            IChangeLogManipulator changelog,
            ICommandService commandService,
            ICapiCleanUpService capiCleanUpService,
            ILogger logger,
            ICapiSynchronizationCacheService capiSynchronizationCacheService,
            IJsonUtils jsonUtils,
            IViewFactory<InterviewMetaInfoInputModel, InterviewMetaInfo> interviewIntoFactory,
            IPrincipal principal)
        {
            this.logger = logger;
            this.capiSynchronizationCacheService = capiSynchronizationCacheService;
            this.jsonUtils = jsonUtils;
            this.interviewIntoFactory = interviewIntoFactory;
            this.principal = principal;
            this.changelog = changelog;
            this.commandService = commandService;
            this.capiCleanUpService = capiCleanUpService;
        }

        public void ProcessDownloadedPackage(InterviewSyncPackageDto item, string itemType)
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
                InterviewMetaInfo interviewMetaInfo = this.interviewIntoFactory.Load(new InterviewMetaInfoInputModel(interviewId));
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
            var metaInfo = this.jsonUtils.Deserialize<InterviewMetaInfo>(item.MetaInfo);
            try
            {
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

        //private void DeleteQuestionnaire(QuestionnaireSyncPackageDto item)
        //{
        //    QuestionnaireMetadata metadata;
        //    try
        //    {
        //        metadata = this.jsonUtils.Deserialize<QuestionnaireMetadata>(item.MetaInfo);
        //    }
        //    catch (Exception exception)
        //    {
        //        throw new ArgumentException("Failed to extract questionnaire version. Please upgrade supervisor to the latest version.", exception);
        //    }
            
        //    try
        //    {
        //        this.commandService.Execute(new DisableQuestionnaire(metadata.QuestionnaireId, metadata.Version, null));

        //        var questionnaireIdentity = new QuestionnaireIdentity(metadata.QuestionnaireId, metadata.Version);

        //        this.questionnaireRepository.DeleteQuestionnaireDocument(metadata.QuestionnaireId, metadata.Version);
        //        this.questionnareAssemblyFileAccessor.RemoveAssembly(metadata.QuestionnaireId, metadata.Version);
        //        this.questionnaireModelRepository.Remove(questionnaireIdentity.ToString());

        //        this.commandService.Execute(new DeleteQuestionnaire(metadata.QuestionnaireId, metadata.Version, null));
        //    }
        //    catch (Exception exception)
        //    {
        //        this.logger.Warn(
        //            string.Format("Failed to execute questionnaire deletion command (id: {0}, version: {1}).", metadata.QuestionnaireId.FormatGuid(), metadata.Version),
        //            exception);
        //    }
        //}        
    }
}