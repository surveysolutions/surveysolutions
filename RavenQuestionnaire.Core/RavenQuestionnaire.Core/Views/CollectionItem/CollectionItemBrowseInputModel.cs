using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    public class CollectionItemBrowseInputModel
    {
        public string CollectionId { get; set; }

        public Guid QuestionId { get; set; }

        public CollectionItemBrowseInputModel(string collectionId, Guid QuestionId)
        {
            this.CollectionId = IdUtil.CreateCollectionId(collectionId);

            this.QuestionId = QuestionId;
        }
    }
}
