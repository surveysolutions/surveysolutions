using System.Text;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;
    using Main.Core.Documents;
    using Main.DenormalizerStorage;
    using Newtonsoft.Json;
    using SynchronizationMessages.Synchronization;

    public class SyncProvider : ISyncProvider
    {
        private const bool UseCompression = true;

        //compressed content could be more than uncompressed for small items 
        private int limitLengtForCompression = 0;

        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> questionnaires;

        private readonly IDenormalizerStorage<UserDocument> users;

        public SyncProvider(IDenormalizerStorage<CompleteQuestionnaireStoreDocument> surveys, 
            IDenormalizerStorage<UserDocument> users)
        {
            this.questionnaires = surveys;
            this.users = users;
        }

        public SyncItem GetSyncItem(Guid id, string type)
        {
            switch (type)
            {
                case SyncItemType.File:
                    return null;
                    break;
                case SyncItemType.Questionnare:
                    return GetItem(CreateQuestionnarieDocument(id), id, type);
                    break;
                case SyncItemType.User:
                    return GetItem(this.users.GetById(id), id, type);
                    break;
                default:
                    return null;
            }

            return null;
        }

        private CompleteQuestionnaireDocument CreateQuestionnarieDocument(Guid id)
        {
            var retval = new CompleteQuestionnaireDocument();
            var data = this.questionnaires.GetById(id);

            retval.CreatedBy = data.CreatedBy;
            retval.CreationDate = data.CreationDate;
            retval.Creator = data.Creator;
            retval.LastEntryDate = data.LastEntryDate;
            retval.PublicKey = data.PublicKey;
            retval.Responsible = data.Responsible;
            retval.Status = data.Status;
            retval.TemplateId = data.TemplateId;
            retval.Title = data.Title;
            
            retval.Children = data.Children;
            return retval;
        }

        private SyncItem GetItem(object item, Guid id, string type)
        {
            if (item == null)
            {
                return null;
            }

            var result = new SyncItem {Id = id, 
                Content = GetItemContent(item), 
                ItemType = type, 
                IsCompressed = UseCompression};

            return result;

        }

        private string GetItemContent(object item)
        {
            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Objects};
            string itemToSync = JsonConvert.SerializeObject(item, Formatting.None, settings);
            
            if (UseCompression)
            {
                return PackageHelper.CompressString(itemToSync);
            }
            else
            {
                return itemToSync;
            }
        }

    }
}
