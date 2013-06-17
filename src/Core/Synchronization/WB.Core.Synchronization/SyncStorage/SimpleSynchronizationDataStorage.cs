using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core;
using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Core.Infrastructure;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization.SyncStorage
{
    public class SimpleSynchronizationDataStorage : ISynchronizationDataStorage
    {
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> questionnarieStorage;
        private readonly IChunkStorage fileChunkStorage;

        public SimpleSynchronizationDataStorage(
            IDenormalizerStorage<CompleteQuestionnaireStoreDocument> questionnarieStorage,
            IChunkStorage fileChunkStorage)
        {
            this.questionnarieStorage = questionnarieStorage;
            this.fileChunkStorage = fileChunkStorage;
        }

        public void SaveQuestionnarie(Guid id)
        {
            var syncItem = GetItem(null, id, SyncItemType.Questionnare);
            fileChunkStorage.StoreChunk(id, GetItemAsContent(syncItem));
        }

        public void SaveUser(UserDocument doc)
        {

            var syncItem = GetItem(doc, doc.PublicKey, SyncItemType.User);
            fileChunkStorage.StoreChunk(doc.PublicKey, GetItemAsContent(syncItem));
        }

        public SyncItem GetLatestVersion(Guid id)
        {
            var result = JsonConvert.DeserializeObject<SyncItem>(fileChunkStorage.ReadChunk(id),
                                                           new JsonSerializerSettings
                                                               {
                                                                   TypeNameHandling = TypeNameHandling.Objects
                                                               });
            if (result.ItemType == SyncItemType.Questionnare)
            {
                result.Content = GetItemAsContent(CreateQuestionnarieDocument(id));
            }
            if (UseCompression)
                result.Content = PackageHelper.CompressString(result.Content);
            return result;
        }

       
        #region from sync provider


        private SyncItem GetItem(object item, Guid id, string type)
        {
            var result = new SyncItem
            {
                Id = id,
                ItemType = type,
                IsCompressed = UseCompression
            };
            if (item != null)
                result.Content = GetItemAsContent(item);
            return result;
        }

       

        private CompleteQuestionnaireDocument CreateQuestionnarieDocument(Guid id)
        {
            var retval = new CompleteQuestionnaireDocument();
            var data = this.questionnarieStorage.GetById(id);

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


        private string GetItemAsContent(object item)
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };
            string itemToSync = JsonConvert.SerializeObject(item, Formatting.None, settings);

            return itemToSync;
        }

        private const bool UseCompression = true;

        #endregion
    }
}
