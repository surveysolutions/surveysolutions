using System;
using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.CollectionItem
{
    public class CollectionItemBrowseView
    {
        public string CollectionId { get; set; }
        public List<CollectionItemBrowseItem> Items { get; set; }
        public Guid PublicKey { get; set; }
        public Guid QuestionId { get; set; }


        public CollectionItemBrowseView(string collectionId, List<CollectionItemBrowseItem> items, Guid questionId)
        {
            this.CollectionId = collectionId;
            this.Items = items;
            this.QuestionId = questionId;
            this.PublicKey=new Guid();
        }
    }
}
