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
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Services;

namespace WB.UI.Capi.Syncronization.Pull
{
    public class PullDataProcessor
    {
        public PullDataProcessor(IChangeLogManipulator changelog, ICommandService commandService, IReadSideRepositoryReader<LoginDTO> userStorage)
        {
            this.logger = ServiceLocator.Current.GetInstance<ILogger>();
            this.changelog = changelog;
            this.commandService = commandService;
            this.cleanUpExecutor = new CleanUpExecutor(changelog);
            this.userStorage = userStorage;
        }
        private ILogger logger;
        private CleanUpExecutor cleanUpExecutor;

        private readonly IChangeLogManipulator changelog;
        private readonly ICommandService commandService;
        private readonly IReadSideRepositoryReader<LoginDTO> userStorage;

        
        public void Proccess(SyncItem item)
        {
            switch (item.ItemType)
            {
                case SyncItemType.Questionnare:
                    this.ExecuteInterview(item);
                    break;
                case SyncItemType.DeleteQuestionnare:
                    this.ExecuteDeleteQuestionnarie(item);
                    break;
                case SyncItemType.User:
                    this.ExecuteUser(item);
                    break;
                case SyncItemType.File:
                    this.ExecuteFile(item);
                    break;
                case SyncItemType.Template:
                    this.ExecuteTemplate(item);
                    break;
                default: break;
            }

            this.changelog.CreatePublicRecord(item.Id);
        }

        private void ExecuteFile(SyncItem item)
        {
            string content = item.IsCompressed ? PackageHelper.DecompressString(item.Content) : item.Content;

            var file = JsonUtils.GetObject<FileSyncDescription>(content);
            this.commandService.Execute(new UploadFileCommand(file.PublicKey, file.Title, file.Description, file.OriginalFile));
        }

        private void ExecuteDeleteQuestionnarie(SyncItem item)
        {
            string content = item.IsCompressed ? PackageHelper.DecompressString(item.Content) : item.Content;
            var questionnarieId = Guid.Parse(content);

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

        private void ExecuteUser(SyncItem item)
        {
            string content = item.IsCompressed ? PackageHelper.DecompressString(item.Content) : item.Content;
            var user = JsonUtils.GetObject<UserDocument>(content);

            ICommand userCommand = null;

            if (this.userStorage.GetById(user.PublicKey) == null)
                userCommand = new CreateUserCommand(user.PublicKey, user.UserName, user.Password, user.Email,
                                                    user.Roles.ToArray(), user.IsLocked, user.Supervisor);

            else

                userCommand = new ChangeUserCommand(user.PublicKey, user.Email,
                                                    user.Roles.ToArray(), user.IsLocked, user.Password);
            this.commandService.Execute(userCommand);
        }

        private void ExecuteInterview(SyncItem item)
        {
            string meta = item.IsCompressed ? PackageHelper.DecompressString(item.MetaInfo) : item.MetaInfo;

            var metaInfo = JsonUtils.GetObject<InterviewMetaInfo>(meta);
            
            var syncCacher = CapiApplication.Kernel.Get<ISyncCacher>();
            syncCacher.SaveItem(metaInfo.PublicKey, item.Content);

            this.commandService.Execute(new ApplySynchronizationMetadata(metaInfo.PublicKey, metaInfo.ResponsibleId, metaInfo.TemplateId,
                (InterviewStatus) metaInfo.Status,
                metaInfo.FeaturedQuestionsMeta.Select(
                    q =>
                        new AnsweredQuestionSynchronizationDto(
                            q.PublicKey, new decimal[0], q.Value,
                            string.Empty))
                    .ToArray(), string.Empty, true));
        }

        private void ExecuteTemplate(SyncItem item)
        {
            string content = item.IsCompressed ? PackageHelper.DecompressString(item.Content) : item.Content;
            var template = JsonUtils.GetObject<QuestionnaireDocument>(content);
            this.commandService.Execute(new ImportFromSupervisor(template));
        }
    }
}