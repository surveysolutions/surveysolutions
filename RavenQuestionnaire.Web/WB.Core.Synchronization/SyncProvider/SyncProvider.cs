using System.Text;

namespace WB.Core.Synchronization.SyncProvider
{
    using System;
    using Main.Core.Documents;
    using Main.Core.Utility;
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
                case "f":
                    return null;
                    break;
                case "q":
                    return GetItem(this.questionnaires.GetById(id), id, type);
                    break;
                case "u":
                    return GetItem(this.users.GetById(id), id, type);
                    break;
                default:
                    return null;
            }

            return null;
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
                return Encoding.UTF8.GetString(PackageHelper.Compress(itemToSync));
            }
            else
            {
                return itemToSync;
            }
        }

    }
}
