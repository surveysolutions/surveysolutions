using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Synchronization
{
    public class SyncProgressView
    {
        public SyncProgressView(SyncProcessDocument doc)
        {
            this.ProcessPublicKey = doc.PublicKey;
            this.StartDate = doc.StartDate;
            this.EndDate = doc.EndDate;
            if(doc.AggregateRoots.Count==0)
            {
                this.StateDescription = "Handshake";
                return;
            }
            if(doc.EndDate.HasValue)
            {
                this.StateDescription = "Process is finished";
                this.ProgressPercentage = 100;
                return;
            }
            this.StateDescription = "Retrieving documents";
            var initialStateEventsCount = doc.AggregateRoots.Count(e => e.Handled == EventState.Initial);
            this.ProgressPercentage = (int)(((decimal)(doc.AggregateRoots.Count - initialStateEventsCount) / doc.AggregateRoots.Count) * 100);
        }

        public string StateDescription { get; set; }
        public Guid ProcessPublicKey { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProgressPercentage { get; set; }
    }
}
