using System;
using System.Collections.Generic;
using System.Linq;
using CAPI.Android.Core.Model;
using CAPI.Android.Core.Model.EventHandlers;
using CAPI.Android.Core.Model.ModelUtils;
using CAPI.Android.Core.Model.SyncCacher;
using CAPI.Android.Core.Model.ViewModel.Dashboard;
using Main.Core;
using Main.Core.Commands.File;
using Main.Core.Commands.Questionnaire.Completed;
using Main.Core.Commands.User;
using Main.Core.Documents;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
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
            
            HandleItem(item);

            changelog.CreatePublicRecord(item.Id);

            chuncksFroProccess.Remove(item);
        }
        
        protected void HandleItem(SyncItem item)
        {
            switch (item.ItemType)
            {
                case SyncItemType.Questionnare:
                    ExecuteInterview(item);
                    break;
                case SyncItemType.DeleteQuestionnare:
                    ExecuteDeleteQuestionnarie(item);
                    break;
                case SyncItemType.User:
                    ExecuteUser(item);
                    break;
                case SyncItemType.File:
                    ExecuteFile(item);
                    break;
                default: break;
            }
        
        }

        private void ExecuteFile(SyncItem item)
        {
            string content = item.IsCompressed ? PackageHelper.DecompressString(item.Content) : item.Content;

            var file = JsonUtils.GetObject<FileSyncDescription>(content);
            commandService.Execute(new UploadFileCommand(file.PublicKey, file.Title, file.Description, file.OriginalFile));
        }

        private void ExecuteDeleteQuestionnarie(SyncItem item)
        {
            string content = item.IsCompressed ? PackageHelper.DecompressString(item.Content) : item.Content;
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

        private void ExecuteUser(SyncItem item)
        {
            string content = item.IsCompressed ? PackageHelper.DecompressString(item.Content) : item.Content;
            var user = JsonUtils.GetObject<UserDocument>(content);
            commandService.Execute(new CreateUserCommand(user.PublicKey, user.UserName, user.Password, user.Email,
                                                         user.Roles.ToArray(), user.IsLocked, user.Supervisor));
        }

        private void ExecuteInterview(SyncItem item)
        {
            if (!string.IsNullOrWhiteSpace(item.MetaInfo))
            {
                string meta = item.IsCompressed ? PackageHelper.DecompressString(item.MetaInfo) : item.MetaInfo;

                var metaInfo = JsonUtils.GetObject<InterviewMetaInfo>(meta);
                
                ////todo: 
                //save item to sync cache for handling on demand
                var syncCacher = CapiApplication.Kernel.Get<ISyncCacher>();
                syncCacher.SaveItem(item.Id, item.Content);

                //apply meta to make chanages on dashboard
                // create elgant solution

                var dashboard = new DashboardDenormalizer(CapiApplication.Kernel.Get<IReadSideRepositoryWriter<QuestionnaireDTO>>(),
                                          CapiApplication.Kernel.Get<IReadSideRepositoryWriter<SurveyDto>>());

                dashboard.ProcessInterviewMeta(metaInfo);

            }
            ////todo: add lazy loadng of Interview
            ////then uncomment 
            else
            {
                string content = item.IsCompressed ? PackageHelper.DecompressString(item.Content) : item.Content;

                var questionnarieContent = JsonUtils.GetObject<CompleteQuestionnaireDocument>(content);
                commandService.Execute(new CreateNewAssigment(questionnarieContent));    
            }
            
        }
    }
}