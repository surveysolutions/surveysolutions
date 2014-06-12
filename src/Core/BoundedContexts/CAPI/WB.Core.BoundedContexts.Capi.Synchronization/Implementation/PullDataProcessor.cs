using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Commands.File;
using Main.Core.Documents;
using Main.Core.View;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Cleaner;
using WB.Core.BoundedContexts.Capi.Synchronization.SyncCacher;
using WB.Core.BoundedContexts.Capi.Synchronization.Views.Login;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Synchronization.Implementation
{
    public class DataProcessor : IDataProcessor
    {
        public DataProcessor(IChangeLogManipulator changelog, ICommandService commandService,
            IViewFactory<LoginViewInput, LoginView> loginViewFactory, IPlainQuestionnaireRepository questionnaireRepository,
            ICleanUpExecutor cleanUpExecutor, ILogger logger, ISyncCacher syncCacher, IStringCompressor stringCompressor, IJsonUtils jsonUtils)
        {
            this.logger = logger;
            this.syncCacher = syncCacher;
            this.stringCompressor = stringCompressor;
            this.jsonUtils = jsonUtils;
            this.changelog = changelog;
            this.commandService = commandService;
            this.cleanUpExecutor = cleanUpExecutor;
            this.loginViewFactory = loginViewFactory;
            this.questionnaireRepository = questionnaireRepository;
        }

        private readonly ILogger logger;
        private readonly ICleanUpExecutor cleanUpExecutor;
        private readonly IChangeLogManipulator changelog;
        private readonly ISyncCacher syncCacher;
        private readonly ICommandService commandService;
        private readonly IViewFactory<LoginViewInput, LoginView> loginViewFactory;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStringCompressor stringCompressor;
        private readonly IJsonUtils jsonUtils;

        public void ProcessPulledItem(SyncItem item)
        {
            switch (item.ItemType)
            {
                case SyncItemType.Questionnare:
                    this.UpdateInterview(item);
                    break;
                case SyncItemType.DeleteQuestionnare:
                    this.DeleteInterview(item);
                    break;
                case SyncItemType.User:
                    this.ChangeOrCreateUser(item);
                    break;
                case SyncItemType.File:
                    this.UploadFile(item);
                    break;
                case SyncItemType.Template:
                    this.UpdateQuestionnaire(item);
                    break;
                default: break;
            }

            this.changelog.CreatePublicRecord(item.Id);
        }

        public IList<ChangeLogRecordWithContent> GetItemsForPush()
        {
            var records = this.changelog.GetClosedDraftChunksIds();
            return records.Select(chunk => new ChangeLogRecordWithContent(chunk.RecordId, chunk.EventSourceId, this.changelog.GetDraftRecordContent(chunk.RecordId))).ToList();
        }

        private void UploadFile(SyncItem item)
        {
            var file = this.ExtractObject<FileSyncDescription>(item.Content, item.IsCompressed);

            this.commandService.Execute(new UploadFileCommand(file.PublicKey, file.Title, file.Description, file.OriginalFile));
        }

        private void DeleteInterview(SyncItem item)
        {
            var questionnarieId = this.ExtractGuid(item.Content, item.IsCompressed);

            try
            {
                this.cleanUpExecutor.DeleteInterveiw(questionnarieId);
            }

            #warning replace catch with propper handler of absent questionnaries
            catch (Exception ex)
            {
                this.logger.Error("Error on item deletion " + questionnarieId, ex);
            }
        }

        private void ChangeOrCreateUser(SyncItem item)
        {
            var user = this.ExtractObject<UserDocument>(item.Content, item.IsCompressed);

            ICommand userCommand = null;

            if (this.loginViewFactory.Load(new LoginViewInput(user.PublicKey)) == null)
                userCommand = new CreateUserCommand(user.PublicKey, user.UserName, user.Password, user.Email,
                                                    user.Roles.ToArray(), user.IsLockedBySupervisor, user.IsLockedByHQ,  user.Supervisor);
            else
                userCommand = new ChangeUserCommand(user.PublicKey, user.Email,
                                                    user.Roles.ToArray(), user.IsLockedBySupervisor, user.IsLockedByHQ, user.Password, Guid.Empty);

            this.commandService.Execute(userCommand);
        }

        private void UpdateInterview(SyncItem item)
        {
            var metaInfo = this.ExtractObject<InterviewMetaInfo>(item.MetaInfo, item.IsCompressed);
            try
            {
                this.syncCacher.SaveItem(metaInfo.PublicKey, item.Content);

                bool createdOnClient = metaInfo.CreatedOnClient.HasValue && metaInfo.CreatedOnClient.Value;

                this.commandService.Execute(new ApplySynchronizationMetadata(metaInfo.PublicKey, metaInfo.ResponsibleId, metaInfo.TemplateId,
                    (InterviewStatus)metaInfo.Status,
                    metaInfo.FeaturedQuestionsMeta.Select(
                        q =>
                            new AnsweredQuestionSynchronizationDto(
                                q.PublicKey, new decimal[0], q.Value,
                                string.Empty))
                        .ToArray(), string.Empty, true, createdOnClient));

            }
            catch (Exception ex)
            {
                this.logger.Error("Error meta applying " + metaInfo.PublicKey, ex);
                throw;
            }
        }

        private void UpdateQuestionnaire(SyncItem item)
        {
            var template = this.ExtractObject<QuestionnaireDocument>(item.Content, item.IsCompressed);

            QuestionnaireMetadata metadata;
            try
            {
                metadata = this.ExtractObject<QuestionnaireMetadata>(item.MetaInfo, item.IsCompressed);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("Failed to extract questionnaire version. Please upgrade supervisor to the latest version.", exception);
            }

            this.questionnaireRepository.StoreQuestionnaire(template.PublicKey, metadata.Version, template);
            this.commandService.Execute(new RegisterPlainQuestionnaire(template.PublicKey, metadata.Version));
        }

        private TResult ExtractObject<TResult>(string initialString, bool isCompressed)
        {
            string stringData = this.ExtractStringData(initialString, isCompressed);

            return this.jsonUtils.Deserrialize<TResult>(stringData);
        }

        private Guid ExtractGuid(string initialString, bool isCompressed)
        {
            string stringData = this.ExtractStringData(initialString, isCompressed);

            return Guid.Parse(stringData);
        }

        private string ExtractStringData(string initialString, bool isCompressed)
        {
            return isCompressed ? this.stringCompressor.DecompressString(initialString) : initialString;
        }
    }
}