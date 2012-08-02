using System;
using System.Collections.Generic;
using Ncqrs;
using Ncqrs.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events;

namespace RavenQuestionnaire.Core.Domain
{
    class CollectionAR : AggregateRootMappedByConvention
    {
        private DateTime _creationDate;
        private string _title;
        private List<CollectionItem> _items;
        
        public CollectionAR ()
        {
        }

        public CollectionAR(Guid collectionId, String text, List<CollectionItem>  items)
            : base(collectionId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewNoteAdded method).
            ApplyEvent(new NewCollectionCreated
            {
                CollectionId = collectionId,
                Title = text,
                Items = items,
                CreationDate = clock.UtcNow()
            });
        }

        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewQuestionnaireCreated(NewCollectionCreated e)
        {
            _title = e.Title;
            _creationDate = e.CreationDate;
            _items = e.Items;
        }

        public void PreLoad()
        {
            //loads into the cache
            //no logic
        }


/*
        public void RemoveCollection(Guid guid)
        {
            ApplyEvent(new CommentSeted()
            {
                     });
        }

        public void RemoveCollectionItem(Guid guid)
        {
            ApplyEvent(new CommentSeted()
            {
                     });
        }*/

        

    }
}
