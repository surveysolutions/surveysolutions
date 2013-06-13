using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model.ChangeLog;
using CAPI.Android.Core.Model.ModelUtils;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Commands.User;
using Main.Core.Documents;
using Ncqrs.Commanding.CommandExecution;
using Ncqrs.Commanding.ServiceModel;
using Ncqrs.Eventing.ServiceModel.Bus;
using Newtonsoft.Json;
using SynchronizationMessages.Synchronization;
using WB.Core.Synchronization;

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

        public void Proccess(Guid chunkId)
        {

            var item = chuncksFroProccess.FirstOrDefault(i => i.Id == chunkId);
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
                case SyncItemType.User:
                    ExecuteUser(content);
                    break;
                default: break;
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