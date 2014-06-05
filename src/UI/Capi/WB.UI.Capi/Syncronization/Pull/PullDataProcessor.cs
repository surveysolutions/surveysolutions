using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.SyncCacher;
using CAPI.Android.Core.Model.ViewModel.Login;
using Main.Core;
using Main.Core.Commands.File;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.BoundedContexts.Capi.ModelUtils;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Services;

namespace WB.UI.Capi.Syncronization.Pull
{
    public class PullDataProcessor
    {
        public PullDataProcessor(IChangeLogManipulator changelog, ICommandService commandService, IReadSideRepositoryReader<LoginDTO> userStorage, IPlainQuestionnaireRepository questionnaireRepository)
        {
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
            this.changelog = changelog;
            this.commandService = commandService;
            this.cleanUpExecutor = new CleanUpExecutor(changelog);
            this.userStorage = userStorage;
            this.questionnaireRepository = questionnaireRepository;
        }

        private readonly ILogger logger;
        private readonly CleanUpExecutor cleanUpExecutor;

        private readonly IChangeLogManipulator changelog;
        private readonly ICommandService commandService;
        private readonly IReadSideRepositoryReader<LoginDTO> userStorage;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;


        public void Proccess(SyncItem item)
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

        private void UploadFile(SyncItem item)
        {
            var file = ExtractObject<FileSyncDescription>(item.Content, item.IsCompressed);

            this.commandService.Execute(new UploadFileCommand(file.PublicKey, file.Title, file.Description, file.OriginalFile));
        }

        private void DeleteInterview(SyncItem item)
        {
            var questionnarieId = ExtractGuid(item.Content, item.IsCompressed);

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
            var user = ExtractObject<UserDocument>(item.Content, item.IsCompressed);

            ICommand userCommand = null;

            if (this.userStorage.GetById(user.PublicKey) == null)
                userCommand = new CreateUserCommand(user.PublicKey, user.UserName, user.Password, user.Email,
                                                    user.Roles.ToArray(), user.IsLockedBySupervisor, user.IsLockedByHQ,  user.Supervisor);
            else
                userCommand = new ChangeUserCommand(user.PublicKey, user.Email,
                                                    user.Roles.ToArray(), user.IsLockedBySupervisor, user.IsLockedByHQ, user.Password, Guid.Empty);

            this.commandService.Execute(userCommand);
        }

        private void UpdateInterview(SyncItem item)
        {
            var metaInfo = ExtractObject<InterviewMetaInfo>(item.MetaInfo, item.IsCompressed);
            
            var syncCacher = CapiApplication.Kernel.Get<ISyncCacher>();

            try
            {
                syncCacher.SaveItem(metaInfo.PublicKey, item.Content);

                this.commandService.Execute(new ApplySynchronizationMetadata(metaInfo.PublicKey, metaInfo.ResponsibleId, metaInfo.TemplateId,
                    (InterviewStatus)metaInfo.Status,
                    metaInfo.FeaturedQuestionsMeta.Select(
                        q =>
                            new AnsweredQuestionSynchronizationDto(
                                q.PublicKey, new decimal[0], q.Value,
                                string.Empty))
                        .ToArray(), string.Empty, true, metaInfo.CreatedOnClient));

            }
            catch (Exception ex)
            {
                this.logger.Error("Error meta applying " + metaInfo.PublicKey, ex);
                throw;
            }
        }

        private void UpdateQuestionnaire(SyncItem item)
        {
            var template = ExtractObject<QuestionnaireDocument>(item.Content, item.IsCompressed);

            QuestionnaireMetadata metadata;
            try
            {
                metadata = ExtractObject<QuestionnaireMetadata>(item.MetaInfo, item.IsCompressed);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("Failed to extract questionnaire version. Please upgrade supervisor to the latest version.", exception);
            }

            this.questionnaireRepository.StoreQuestionnaire(template.PublicKey, metadata.Version, template);
            this.commandService.Execute(new RegisterPlainQuestionnaire(template.PublicKey, metadata.Version));
        }

        private static TResult ExtractObject<TResult>(string initialString, bool isCompressed)
        {
            string stringData = ExtractStringData(initialString, isCompressed);

            return JsonUtils.GetObject<TResult>(stringData);
        }

        private static Guid ExtractGuid(string initialString, bool isCompressed)
        {
            string stringData = ExtractStringData(initialString, isCompressed);

            return Guid.Parse(stringData);
        }

        private static string ExtractStringData(string initialString, bool isCompressed)
        {
            return isCompressed ? PackageHelper.DecompressString(initialString) : initialString;
        }
    }
}