using System;
using Ncqrs;
using Ncqrs.Domain;
using RavenQuestionnaire.Core.Events;

namespace RavenQuestionnaire.Core.Domain
{
    public class LocationAR : AggregateRootMappedByConvention
    {
        private DateTime _creationDate;
        private string _title;
        
        public LocationAR ()
        {
        }

        public LocationAR(Guid locationId, String text)
            : base(locationId)
        {
            var clock = NcqrsEnvironment.Get<IClock>();

            // Apply a NewQuestionnaireCreated event that reflects the
            // creation of this instance. The state of this
            // instance will be update in the handler of 
            // this event (the OnNewNoteAdded method).
            ApplyEvent(new NewLocationCreated
            {
                LocationId = locationId,
                Title = text,
                CreationDate = clock.UtcNow()
            });
        }

        // Event handler for the NewQuestionnaireCreated event. This method
        // is automaticly wired as event handler based on convension.
        protected void OnNewQuestionnaireCreated(NewLocationCreated e)
        {
            _title = e.Title;
            _creationDate = e.CreationDate;
        }
    }
}
