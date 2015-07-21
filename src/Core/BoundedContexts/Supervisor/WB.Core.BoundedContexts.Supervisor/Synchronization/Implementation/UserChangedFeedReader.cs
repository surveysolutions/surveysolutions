using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom.Implementation;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class UserChangedFeedReader : IUserChangedFeedReader
    {
        private readonly IHeadquartersSettings headquartersSettings;
        private readonly HeadquartersPullContext headquartersPullContext;
        private readonly IAtomFeedReader atomReader;

        public UserChangedFeedReader(IHeadquartersSettings headquartersSettings, 
            Func<HttpMessageHandler> messageHandler,
            HeadquartersPullContext headquartersPullContext)
        {
            if (headquartersSettings == null) throw new ArgumentNullException("headquartersSettings");
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");
            if (headquartersPullContext == null) throw new ArgumentNullException("headquartersPullContext");

            this.headquartersSettings = headquartersSettings;
            this.headquartersPullContext = headquartersPullContext;
            this.atomReader = new AtomFeedReader(messageHandler, headquartersSettings);
        }

        public async Task<List<LocalUserChangedFeedEntry>> ReadAfterAsync(LocalUserChangedFeedEntry lastStoredFeedEntry)
        {
            try
            {
                string lastStoredEntryId = lastStoredFeedEntry != null ? lastStoredFeedEntry.EntryId : null;

                this.headquartersPullContext.PushMessage(string.Format("Reading users feed from URL: {0}", this.headquartersSettings.UserChangedFeedUrl));

                IEnumerable<AtomFeedEntry<LocalUserChangedFeedEntry>> feedEntries = 
                    await atomReader.ReadAfterAsync<LocalUserChangedFeedEntry>(this.headquartersSettings.UserChangedFeedUrl, lastStoredEntryId)
                        .ConfigureAwait(false);

                this.headquartersPullContext.MarkHqAsReachable();
                this.headquartersPullContext.PushMessage(string.Format("Received {0} events from feed {1}", feedEntries.Count(), this.headquartersSettings.UserChangedFeedUrl));

                var result = from f in feedEntries
                    select new LocalUserChangedFeedEntry(f.Content.SupervisorId, f.Content.EntryId, f.Content.EntryType)
                    {
                        Timestamp = f.Content.Timestamp,
                        ChangedUserId = f.Content.ChangedUserId,
                        UserDetailsUri = f.Links.First(l => l.Rel == "enclosure").Href
                    };
                return result.ToList();
            }
            catch (HttpRequestException)
            {
                this.headquartersPullContext.MarkHqAsUnReachable();
                throw;
            }
        }
    }

}