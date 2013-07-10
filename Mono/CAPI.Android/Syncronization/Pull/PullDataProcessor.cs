using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Core.Model.ModelUtils;
using Main.Core;
using Main.Core.Commands.File;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Commands.User;
using Main.Core.Documents;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace CAPI.Android.Syncronization.Pull
{
    public class PullDataProcessor
    {
        private const string syncTemp = "sync_temp";

        public PullDataProcessor(IChangeLogManipulator changelog, ICommandService commandService)
        {
            this.changelog = changelog;
            this.commandService = commandService;
            this.chuncksFroProccess=new List<SyncItem>();
        }
        private IList<SyncItem> chuncksFroProccess;
        private readonly IChangeLogManipulator changelog;
        private readonly ICommandService commandService;

        public void Save(SyncItem  data)
        {
            chuncksFroProccess.Add(data);
        }

        public void Proccess(KeyValuePair<long,Guid> chunkId)
        {

            var item = chuncksFroProccess.FirstOrDefault(i => i.Id == chunkId.Value);
            if (item == null)
                return;

            string unzippedData = item.IsCompressed ? PackageHelper.DecompressString(item.Content) : item.Content;
        
            if(string.IsNullOrEmpty(unzippedData))
                return;

            ExecuteCommand(unzippedData, item);

            changelog.CreatePublicRecord(item.Id);

            chuncksFroProccess.Remove(item);
        }
        
        protected void ExecuteCommand(string content, SyncItem item)
        {
            switch (item.ItemType)
            {
                case SyncItemType.Questionnare:
                    ExecuteQuestionnarie(content);
                    break;
                case SyncItemType.DeleteQuestionnare:
                    ExecuteDeleteQuestionnarie(content);
                    break;
                case SyncItemType.User:
                    ExecuteUser(content);
                    break;
                case SyncItemType.File:
                    ExecuteFile(content);
                    break;
                default: break;
            }
        
        }

        private void ExecuteFile(string content)
        {
            var file = JsonUtils.GetObject<FileSyncDescription>(content);
            commandService.Execute(new UploadFileCommand(file.PublicKey, file.Title, file.Description, file.OriginalFile));
        }

        private void ExecuteDeleteQuestionnarie(string content)
        {
            var questionnarieId = Guid.Parse(content);

            try
            {
                commandService.Execute(new DeleteCompleteQuestionnaireCommand(questionnarieId));
            }
#warning replace catch with propper handler of absent questionnaries
            catch (Exception)
            {
                
            }
          
        }

        private void ExecuteUser(string content)
        {
            var user = JsonUtils.GetObject<UserDocument>(content);
            commandService.Execute(new CreateUserCommand(user.PublicKey, user.UserName, user.Password, user.Email,
                                                         user.Roles.ToArray(), user.IsLocked, user.Supervisor));
        }

        private void ExecuteQuestionnarie(string content)
        {
            var questionnarieContent = JsonUtils.GetObject<CompleteQuestionnaireDocument>(content);
            commandService.Execute(new CreateNewAssigment(questionnarieContent));
        }
    }
}