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
            switch (doc.Handled)
            {
                case EventState.Initial:
                    this.StateDescription = "Handshake";
                    this.ProgressPercentage = 0;
                    break;
                case EventState.Error:
                    this.StateDescription = "Process is finished with errors";
                    this.ProgressPercentage = -1;
                    break;
                case EventState.Completed:
                    this.StateDescription = "Process is finished";
                    this.ProgressPercentage = 100;
                    break;
                case EventState.InProgress:
                    this.StateDescription = "Retrieving documents";
                    var initialStateEventsCount = doc.Chunks.Count(e => e.Handled == EventState.Initial);
                    this.ProgressPercentage = doc.Chunks.Count == 0 ? 100 : (int)(initialStateEventsCount * 100 / doc.Chunks.Count);
                        //(((decimal)(doc.Chunks.Count - initialStateEventsCount) / doc.Chunks.Count) * 100);
                    // process can't display 100% when it's not marked as completed
                    if (this.ProgressPercentage == 100)
                        this.ProgressPercentage = 99;
                    break;
            }
        }

        public string StateDescription { get; set; }
        public Guid ProcessPublicKey { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ProgressPercentage { get; set; }
    }
}
