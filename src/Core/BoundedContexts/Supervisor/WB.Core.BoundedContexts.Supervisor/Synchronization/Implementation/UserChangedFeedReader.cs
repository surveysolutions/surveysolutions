using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Raven.Client.Linq.Indexing;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom.Implementation;

namespace WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation
{
    internal class UserChangedFeedReader : IUserChangedFeedReader
    {
        private readonly HeadquartersSettings headquartersSettings;
        private readonly IAtomFeedReader atomReader;

        public UserChangedFeedReader(HeadquartersSettings headquartersSettings, HttpMessageHandler messageHandler)
        {
            if (headquartersSettings == null) throw new ArgumentNullException("headquartersSettings");
            if (messageHandler == null) throw new ArgumentNullException("messageHandler");

            this.headquartersSettings = headquartersSettings;
            this.atomReader = new AtomFeedReader(messageHandler);
        }

        public async Task<List<LocalUserChangedFeedEntry>> ReadAfterAsync(LocalUserChangedFeedEntry lastStoredFeedEntry)
        {
            string lastStoredEntryId = lastStoredFeedEntry != null ? lastStoredFeedEntry.EntryId : null;
            IEnumerable<AtomFeedEntry<LocalUserChangedFeedEntry>> feedEntries = 
                await atomReader.ReadAfterAsync<LocalUserChangedFeedEntry>(this.headquartersSettings.UserChangedFeedUrl, lastStoredEntryId)
                                .ConfigureAwait(false);

            var result = from f in feedEntries
                select new LocalUserChangedFeedEntry(f.Content.SupervisorId, f.Content.EntryId)
                {
                    Timestamp = f.Content.Timestamp,
                    ChangedUserId = f.Content.ChangedUserId,
                    UserDetailsUri = f.Links.First(l => l.Rel == "enclosure").Href
                };
            return result.ToList();
        }
    }

}