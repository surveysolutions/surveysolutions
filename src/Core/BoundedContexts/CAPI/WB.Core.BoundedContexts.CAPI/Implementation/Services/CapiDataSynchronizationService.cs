using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Services;
using WB.Core.BoundedContexts.Capi.Views.Login;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Capi.Implementation.Services
{
    public class CapiDataSynchronizationService : ICapiDataSynchronizationService
    {
        public CapiDataSynchronizationService(
            IChangeLogManipulator changelog, 
            ICommandService commandService,
            IViewFactory<LoginViewInput, LoginView> loginViewFactory, 
            IPlainQuestionnaireRepository questionnaireRepository,
            ICapiCleanUpService capiCleanUpService,
            ILogger logger,
            ICapiSynchronizationCacheService capiSynchronizationCacheService, 
            IJsonUtils jsonUtils, 
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor)
        {
            this.logger = logger;
            this.capiSynchronizationCacheService = capiSynchronizationCacheService;
            this.jsonUtils = jsonUtils;
            this.changelog = changelog;
            this.commandService = commandService;
            this.capiCleanUpService = capiCleanUpService;
            this.loginViewFactory = loginViewFactory;
            this.questionnaireRepository = questionnaireRepository;
            this.questionnareAssemblyFileAccessor = questionnareAssemblyFileAccessor;
        }

        private readonly ILogger logger;
        private readonly ICapiCleanUpService capiCleanUpService;
        private readonly IChangeLogManipulator changelog;
        private readonly ICapiSynchronizationCacheService capiSynchronizationCacheService;
        private readonly ICommandService commandService;
        private readonly IViewFactory<LoginViewInput, LoginView> loginViewFactory;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IJsonUtils jsonUtils;

        private readonly IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor;

        public void ProcessDownloadedPackage(UserSyncPackageDto item)
        {
            var user = this.jsonUtils.Deserialize<UserDocument>(item.Content);

            ICommand userCommand = null;

            if (this.loginViewFactory.Load(new LoginViewInput(user.PublicKey)) == null)
                userCommand = new CreateUserCommand(user.PublicKey, user.UserName, user.Password, user.Email,
                    user.Roles.ToArray(), user.IsLockedBySupervisor, user.IsLockedByHQ,  user.Supervisor);
            else
                userCommand = new ChangeUserCommand(user.PublicKey, user.Email,
                    user.Roles.ToArray(), user.IsLockedBySupervisor, user.IsLockedByHQ, user.Password, Guid.Empty);

            this.commandService.Execute(userCommand);
        }

        public void ProcessDownloadedPackage(QuestionnaireSyncPackageDto item)
        {
            switch (item.ItemType)
            {
                case SyncItemType.Questionnaire:
                    this.UpdateQuestionnaire(item);
                    break;
                case SyncItemType.DeleteQuestionnaire:
                    this.DeleteQuestionnaire(item);
                    break;
                case SyncItemType.QuestionnaireAssembly:
                    this.UpdateAssembly(item);
                    break;
            }
        }

        public void ProcessDownloadedPackage(InterviewSyncPackageDto item)
        {
            switch (item.ItemType)
            {
                case SyncItemType.Interview:
                    this.UpdateInterview(item);
                    break;
                case SyncItemType.DeleteInterview:
                    this.DeleteInterview(item);
                    break;
            }
        }

        public IList<ChangeLogRecordWithContent> GetItemsForPush()
        {
            var records = this.changelog.GetClosedDraftChunksIds();
            return records.Select(chunk => new ChangeLogRecordWithContent(chunk.RecordId, chunk.EventSourceId, this.changelog.GetDraftRecordContent(chunk.RecordId))).ToList();
        }

        private void DeleteInterview(InterviewSyncPackageDto item)
        {
            var questionnarieId = Guid.Parse(item.Content);

            try
            {
                this.capiCleanUpService.DeleteInterview(questionnarieId);
            }

            #warning replace catch with propper handler of absent questionnaries
            catch (Exception ex)
            {
                this.logger.Error("Error on item deletion " + questionnarieId, ex);
                throw;
            }
        }

        private void UpdateInterview(InterviewSyncPackageDto item)
        {
            var metaInfo = this.jsonUtils.Deserialize<WB.Core.SharedKernel.Structures.Synchronization.InterviewMetaInfo>(item.MetaInfo);
            try
            {
                bool createdOnClient = metaInfo.CreatedOnClient.HasValue && metaInfo.CreatedOnClient.Value;

                var featuredQuestionsMeta = metaInfo
                    .FeaturedQuestionsMeta
                    .Select(q => new AnsweredQuestionSynchronizationDto(q.PublicKey, new decimal[0], q.Value, string.Empty))
                    .ToArray();

                var applySynchronizationMetadata = new ApplySynchronizationMetadata(
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
                this.logger.Error("Error meta applying " + metaInfo.PublicKey, ex);
                throw;
            }
        }

        private void UpdateQuestionnaire(QuestionnaireSyncPackageDto item)
        {
            var template = this.jsonUtils.Deserialize<QuestionnaireDocument>(item.Content);

            QuestionnaireMetadata metadata;
            try
            {
                metadata =  this.jsonUtils.Deserialize<QuestionnaireMetadata>(item.MetaInfo);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("Failed to extract questionnaire version. Please upgrade supervisor to the latest version.", exception);
            }

            this.questionnaireRepository.StoreQuestionnaire(template.PublicKey, metadata.Version, template);
            
            this.commandService.Execute(new RegisterPlainQuestionnaire(template.PublicKey, metadata.Version, metadata.AllowCensusMode, string.Empty));
        }

        private void DeleteQuestionnaire(QuestionnaireSyncPackageDto item)
        {
            QuestionnaireMetadata metadata;
            try
            {
                metadata = this.jsonUtils.Deserialize<QuestionnaireMetadata>(item.MetaInfo);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("Failed to extract questionnaire version. Please upgrade supervisor to the latest version.", exception);
            }

            this.questionnaireRepository.DeleteQuestionnaireDocument(metadata.QuestionnaireId, metadata.Version);
            this.questionnareAssemblyFileAccessor.RemoveAssembly(metadata.QuestionnaireId, metadata.Version);
            
            try
            {

                this.commandService.Execute(new DeleteQuestionnaire(metadata.QuestionnaireId, metadata.Version, null));
            }
            catch (Exception exception)
            {
                this.logger.Warn(
                    string.Format("Failed to execute questionnaire deletion command (id: {0}, version: {1}).", metadata.QuestionnaireId.FormatGuid(), metadata.Version),
                    exception);
            }
        }

        private void UpdateAssembly(QuestionnaireSyncPackageDto item)
        {
            QuestionnaireAssemblyMetadata metadata;
            try
            {
                metadata = this.jsonUtils.Deserialize<QuestionnaireAssemblyMetadata>(item.MetaInfo);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("Failed to extract questionnaire version. Please upgrade supervisor to the latest version.", exception);
            }

            var assemblyBody = item.Content;

            questionnareAssemblyFileAccessor.StoreAssembly(metadata.QuestionnaireId, metadata.Version, assemblyBody);
        }        
    }
}